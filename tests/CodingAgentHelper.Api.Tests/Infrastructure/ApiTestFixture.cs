namespace CodingAgentHelper.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CodingAgentHelper.Core.Infrastructure.Data;
using Xunit;

/// <summary>
/// Base class for API integration tests with WebApplicationFactory
/// </summary>
public class ApiTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Set test environment - Program.cs will use InMemory database for Test environment
                builder.UseEnvironment("Test");
            });

        Client = _factory.CreateClient();
        
        // Initialize database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CodingAgentDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get the database context for test setup
    /// </summary>
    public CodingAgentDbContext GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CodingAgentDbContext>();
    }
}

/// <summary>
/// Collection definition for API tests
/// </summary>
[CollectionDefinition("API Tests")]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
    // This has no code, just defines the collection
}
