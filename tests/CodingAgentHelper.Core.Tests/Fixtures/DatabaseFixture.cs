namespace CodingAgentHelper.Core.Tests.Fixtures;

using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Test fixture for Entity Framework Core database context
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private CodingAgentDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<CodingAgentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CodingAgentDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    public CodingAgentDbContext GetContext()
    {
        return _context;
    }

    public IStandardRepository GetStandardRepository()
    {
        return new StandardRepository(_context);
    }

    public ICategoryRepository GetCategoryRepository()
    {
        return new CategoryRepository(_context);
    }
}

/// <summary>
/// Collection fixture for database tests to share context
/// </summary>
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to define the collection that tests can join
}
