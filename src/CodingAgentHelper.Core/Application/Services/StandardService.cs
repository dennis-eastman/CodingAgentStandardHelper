namespace CodingAgentHelper.Core.Application.Services;

using Domain.Entities;
using Domain.Repositories;
using Infrastructure.VectorStore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Interface for Standard service operations
/// </summary>
public interface IStandardService
{
    Task<Standard> CreateStandardAsync(string title, string description, string category, CancellationToken cancellationToken = default);
    Task<Standard?> GetStandardAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> GetAllStandardsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> SearchStandardsAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> GetStandardsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> GetStandardsByPriorityAsync(StandardPriority priority, CancellationToken cancellationToken = default);
    Task UpdateStandardAsync(Guid id, string? title, string? description, StandardStatus? status, StandardPriority? priority, CancellationToken cancellationToken = default);
    Task DeleteStandardAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddTagToStandardAsync(Guid standardId, string tag, CancellationToken cancellationToken = default);
    Task RemoveTagFromStandardAsync(Guid standardId, string tag, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing coding standards
/// </summary>
public class StandardService : IStandardService
{
    private readonly IStandardRepository _standardRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IChromaClient _chromaClient;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<StandardService> _logger;
    private readonly ChromaConfiguration _chromaConfig;

    public StandardService(
        IStandardRepository standardRepository,
        ICategoryRepository categoryRepository,
        IChromaClient chromaClient,
        IEmbeddingService embeddingService,
        ChromaConfiguration chromaConfig,
        ILogger<StandardService> logger)
    {
        _standardRepository = standardRepository ?? throw new ArgumentNullException(nameof(standardRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _chromaClient = chromaClient ?? throw new ArgumentNullException(nameof(chromaClient));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _chromaConfig = chromaConfig ?? throw new ArgumentNullException(nameof(chromaConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Standard> CreateStandardAsync(string title, string description, string category, CancellationToken cancellationToken = default)
    {
        try
        {
            var standard = new Standard(title, description, category);
            
            // Add to database
            await _standardRepository.AddAsync(standard, cancellationToken);
            _logger.LogInformation("Created standard '{Title}' with ID {Id}", title, standard.Id);

            // Add to vector store
            await EmbedAndStoreStandardAsync(standard, cancellationToken);

            return standard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating standard '{Title}'", title);
            throw;
        }
    }

    public async Task<Standard?> GetStandardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _standardRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Standard>> GetAllStandardsAsync(CancellationToken cancellationToken = default)
    {
        return await _standardRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Standard>> SearchStandardsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await _standardRepository.GetAllAsync(cancellationToken);

        try
        {
            // Get embedding for query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            
            // Search in vector store
            var collectionName = _chromaConfig.GetCollectionName("standards");
            var vectorResults = await _chromaClient.SearchAsync(collectionName, queryEmbedding, resultLimit: 20, cancellationToken);

            // Get standards by IDs from vector results
            var standardIds = vectorResults
                .Where(r => r.Metadata.TryGetValue("standard_id", out var id) && Guid.TryParse(id, out _))
                .Select(r => Guid.Parse(r.Metadata["standard_id"]))
                .ToList();

            var standards = new List<Standard>();
            foreach (var id in standardIds)
            {
                var standard = await _standardRepository.GetByIdAsync(id, cancellationToken);
                if (standard != null)
                    standards.Add(standard);
            }

            _logger.LogInformation("Vector search returned {Count} standards for query: {Query}", standards.Count, query);
            return standards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search for query: {Query}", query);
            // Fall back to text search
            return await _standardRepository.SearchByTitleOrDescriptionAsync(query, cancellationToken);
        }
    }

    public async Task<IEnumerable<Standard>> GetStandardsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _standardRepository.GetByCategoryAsync(category, cancellationToken);
    }

    public async Task<IEnumerable<Standard>> GetStandardsByPriorityAsync(StandardPriority priority, CancellationToken cancellationToken = default)
    {
        return await _standardRepository.GetByPriorityAsync(priority, cancellationToken);
    }

    public async Task UpdateStandardAsync(Guid id, string? title, string? description, StandardStatus? status, StandardPriority? priority, CancellationToken cancellationToken = default)
    {
        var standard = await _standardRepository.GetByIdAsync(id, cancellationToken);
        if (standard == null)
            throw new KeyNotFoundException($"Standard with ID {id} not found");

        if (status.HasValue)
            standard.UpdateStatus(status.Value);

        if (priority.HasValue)
            standard.UpdatePriority(priority.Value);

        await _standardRepository.UpdateAsync(standard, cancellationToken);
        _logger.LogInformation("Updated standard {Id}", id);
    }

    public async Task DeleteStandardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var standard = await _standardRepository.GetByIdAsync(id, cancellationToken);
        if (standard != null)
        {
            // Remove from vector store if embedded
            if (!string.IsNullOrWhiteSpace(standard.VectorId))
            {
                var collectionName = _chromaConfig.GetCollectionName("standards");
                await _chromaClient.DeleteEmbeddingAsync(collectionName, standard.VectorId, cancellationToken);
            }

            await _standardRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Deleted standard {Id}", id);
        }
    }

    public async Task AddTagToStandardAsync(Guid standardId, string tag, CancellationToken cancellationToken = default)
    {
        var standard = await _standardRepository.GetByIdAsync(standardId, cancellationToken);
        if (standard == null)
            throw new KeyNotFoundException($"Standard with ID {standardId} not found");

        standard.AddTag(tag);
        await _standardRepository.UpdateAsync(standard, cancellationToken);
        _logger.LogInformation("Added tag '{Tag}' to standard {Id}", tag, standardId);
    }

    public async Task RemoveTagFromStandardAsync(Guid standardId, string tag, CancellationToken cancellationToken = default)
    {
        var standard = await _standardRepository.GetByIdAsync(standardId, cancellationToken);
        if (standard == null)
            throw new KeyNotFoundException($"Standard with ID {standardId} not found");

        standard.RemoveTag(tag);
        await _standardRepository.UpdateAsync(standard, cancellationToken);
        _logger.LogInformation("Removed tag '{Tag}' from standard {Id}", tag, standardId);
    }

    private async Task EmbedAndStoreStandardAsync(Standard standard, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create searchable text from standard
            var searchText = $"{standard.Title} {standard.Description} {string.Join(" ", standard.Tags)}";
            
            // Get embedding
            var embedding = await _embeddingService.GetEmbeddingAsync(searchText, cancellationToken);
            
            // Store in vector database
            var collectionName = _chromaConfig.GetCollectionName("standards");
            var vectorEmbedding = new VectorEmbedding
            {
                Id = standard.Id.ToString(),
                Embedding = embedding,
                Metadata = new Dictionary<string, string>
                {
                    { "standard_id", standard.Id.ToString() },
                    { "title", standard.Title },
                    { "category", standard.Category },
                    { "priority", standard.Priority.ToString() }
                },
                Document = standard.Description
            };

            await _chromaClient.AddEmbeddingsAsync(collectionName, new[] { vectorEmbedding }, cancellationToken);
            standard.VectorId = standard.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error embedding standard {StandardId}", standard.Id);
            // Non-blocking: continue even if embedding fails
        }
    }
}
