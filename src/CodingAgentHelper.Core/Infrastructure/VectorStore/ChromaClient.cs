namespace CodingAgentHelper.Core.Infrastructure.VectorStore;

using Microsoft.Extensions.Logging;

/// <summary>
/// Interface for Chroma vector store client operations
/// </summary>
public interface IChromaClient
{
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default);
    Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);
    Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default);
    Task AddEmbeddingsAsync(string collectionName, IEnumerable<VectorEmbedding> embeddings, CancellationToken cancellationToken = default);
    Task<IEnumerable<VectorSearchResult>> SearchAsync(string collectionName, float[] queryEmbedding, int resultLimit = 10, CancellationToken cancellationToken = default);
    Task DeleteEmbeddingAsync(string collectionName, string embeddingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mock Chroma client implementation for Phase 1
/// Stores embeddings in memory for testing
/// Will be replaced with actual Chroma HTTP client in Phase 2
/// </summary>
public class MockChromaClient : IChromaClient
{
    private readonly ChromaConfiguration _config;
    private readonly ILogger<MockChromaClient> _logger;
    private readonly Dictionary<string, List<VectorEmbedding>> _collections;

    public MockChromaClient(ChromaConfiguration config, ILogger<MockChromaClient> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collections = new Dictionary<string, List<VectorEmbedding>>();
    }

    public Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Chroma health check (mock) - OK");
        return Task.FromResult(true);
    }

    public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException("Collection name cannot be empty", nameof(collectionName));

        if (_collections.ContainsKey(collectionName))
        {
            _logger.LogWarning("Collection '{CollectionName}' already exists (mock)", collectionName);
            return Task.CompletedTask;
        }

        _collections[collectionName] = new List<VectorEmbedding>();
        _logger.LogInformation("Created collection '{CollectionName}' (mock)", collectionName);
        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (_collections.Remove(collectionName))
        {
            _logger.LogInformation("Deleted collection '{CollectionName}' (mock)", collectionName);
        }
        return Task.CompletedTask;
    }

    public Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_collections.ContainsKey(collectionName));
    }

    public Task AddEmbeddingsAsync(string collectionName, IEnumerable<VectorEmbedding> embeddings, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collectionName))
            throw new ChromaException($"Collection '{collectionName}' does not exist");

        var embeddingList = embeddings.ToList();
        _collections[collectionName].AddRange(embeddingList);
        _logger.LogInformation("Added {Count} embeddings to collection '{CollectionName}' (mock)", embeddingList.Count, collectionName);
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<VectorSearchResult>> SearchAsync(string collectionName, float[] queryEmbedding, int resultLimit = 10, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collectionName))
            throw new ChromaException($"Collection '{collectionName}' does not exist");

        var collection = _collections[collectionName];
        
        var results = collection
            .Select(e => new
            {
                Embedding = e,
                Distance = CalculateDistance(queryEmbedding, e.Embedding)
            })
            .OrderBy(x => x.Distance)
            .Take(resultLimit)
            .Select(x => new VectorSearchResult
            {
                Id = x.Embedding.Id,
                Distance = x.Distance,
                Metadata = x.Embedding.Metadata,
                Document = x.Embedding.Document
            })
            .ToList();

        _logger.LogInformation("Searched collection '{CollectionName}', returned {Count} results (mock)", collectionName, results.Count);
        return Task.FromResult((IEnumerable<VectorSearchResult>)results);
    }

    public Task DeleteEmbeddingAsync(string collectionName, string embeddingId, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collectionName))
            throw new ChromaException($"Collection '{collectionName}' does not exist");

        var removed = _collections[collectionName].RemoveAll(e => e.Id == embeddingId);
        _logger.LogInformation("Deleted {Count} embeddings from collection '{CollectionName}' (mock)", removed, collectionName);
        
        return Task.CompletedTask;
    }

    private float CalculateDistance(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length)
            throw new ArgumentException("Embeddings must have the same dimension");

        // Euclidean distance
        var sumOfSquares = embedding1.Zip(embedding2, (a, b) => (a - b) * (a - b)).Sum();
        return (float)Math.Sqrt(sumOfSquares);
    }
}
