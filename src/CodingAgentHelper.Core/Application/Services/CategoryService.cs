namespace CodingAgentHelper.Core.Application.Services;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Interface for Category service operations
/// </summary>
public interface ICategoryService
{
    Task<Category> CreateCategoryAsync(string name, string description, CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoriesWithStandardCountAsync(CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Guid id, string description, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing categories
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Category> CreateCategoryAsync(string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if category already exists
            var existing = await _categoryRepository.GetByNameAsync(name, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Category '{name}' already exists");

            var category = new Category(name, description);
            await _categoryRepository.AddAsync(category, cancellationToken);
            _logger.LogInformation("Created category '{Name}' with ID {Id}", name, category.Id);

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category '{Name}'", name);
            throw;
        }
    }

    public async Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return await _categoryRepository.GetByNameAsync(name, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoriesWithStandardCountAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetWithStandardCountAsync(cancellationToken);
    }

    public async Task UpdateCategoryAsync(Guid id, string description, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {id} not found");

        category.UpdateDescription(description);
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        _logger.LogInformation("Updated category {Id}", id);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _categoryRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Deleted category {Id}", id);
    }
}
