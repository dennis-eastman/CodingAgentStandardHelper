namespace CodingAgentHelper.Core.Domain.Repositories;

using Entities;

/// <summary>
/// Base repository interface for all entity repositories
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Standard entities
/// </summary>
public interface IStandardRepository : IRepository<Standard>
{
    Task<IEnumerable<Standard>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> GetByStatusAsync(StandardStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> GetByPriorityAsync(StandardPriority priority, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> SearchByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<IEnumerable<Standard>> SearchByTitleOrDescriptionAsync(string searchTerm, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Category entities
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetWithStandardCountAsync(CancellationToken cancellationToken = default);
}
