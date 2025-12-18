namespace CodingAgentHelper.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
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
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext descriptors
                    var descriptorsToRemove = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<CodingAgentDbContext>) ||
                                   (d.ServiceType == typeof(DbContextOptions) && 
                                    d.ImplementationType?.Name.Contains("CodingAgentDbContext") == true))
                        .ToList();

                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<CodingAgentDbContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()),
                        ServiceLifetime.Transient);
                });
            });

        try
        {
            Client = _factory.CreateClient();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create test client", ex);
        }
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
