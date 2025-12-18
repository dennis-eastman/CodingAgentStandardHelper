namespace CodingAgentHelper.Api.Tests.Controllers;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CodingAgentHelper.Api.Models;
using CodingAgentHelper.Api.Tests.Infrastructure;
using Xunit;

[Collection("API Tests")]
public class StandardsControllerTests
{
    private readonly ApiTestFixture _fixture;

    /// <summary>
    /// JSON serializer options matching ASP.NET Core defaults
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public StandardsControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllStandards_ReturnsOkResult()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/standards");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardListResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetStandard_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "Test Standard",
            Description = "Test Description",
            Category = "Testing"
        };

        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdStandard = JsonSerializer.Deserialize<StandardResponse>(createdContent, JsonOptions);
        Assert.NotNull(createdStandard);

        // Act
        var response = await _fixture.Client.GetAsync($"/api/standards/{createdStandard.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(createdStandard.Id, result!.Id);
        Assert.Equal("Test Standard", result.Title);
    }

    [Fact]
    public async Task GetStandard_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _fixture.Client.GetAsync($"/api/standards/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateStandard_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateStandardRequest
        {
            Title = "New Standard",
            Description = "New Description",
            Category = "Performance"
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/standards", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("New Standard", result!.Title);
        Assert.Equal("New Description", result.Description);
        Assert.Equal("Performance", result.Category);
    }

    [Fact]
    public async Task CreateStandard_WithMissingTitle_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateStandardRequest
        {
            Title = "",
            Description = "Description",
            Category = "Testing"
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/standards", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStandard_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "Original Title",
            Description = "Description",
            Category = "Testing"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdStandard = JsonSerializer.Deserialize<StandardResponse>(createdContent, JsonOptions)!;

        var updateRequest = new UpdateStandardRequest
        {
            Status = 1 // Inactive
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"/api/standards/{createdStandard.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("Inactive", result!.Status);
    }

    [Fact]
    public async Task DeleteStandard_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "To Delete",
            Description = "Description",
            Category = "Testing"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdStandard = JsonSerializer.Deserialize<StandardResponse>(createdContent, JsonOptions)!;

        // Act
        var response = await _fixture.Client.DeleteAsync($"/api/standards/{createdStandard.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's deleted
        var getResponse = await _fixture.Client.GetAsync($"/api/standards/{createdStandard.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task SearchStandards_WithValidQuery_ReturnsMatches()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "Async/Await Pattern",
            Description = "Always use async patterns for I/O",
            Category = "Performance"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        var response = await _fixture.Client.GetAsync("/api/standards/search?query=async");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardListResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.True(result!.TotalCount > 0);
    }

    [Fact]
    public async Task AddTag_WithValidId_AddsTagSuccessfully()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "Tag Test",
            Description = "Testing tags",
            Category = "Testing"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdStandard = JsonSerializer.Deserialize<StandardResponse>(createdContent, JsonOptions)!;

        var tagRequest = new { tag = "important" };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync($"/api/standards/{createdStandard.Id}/tags", tagRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.Contains("important", result!.Tags);
    }

    [Fact]
    public async Task RemoveTag_WithValidIdAndTag_RemovesTagSuccessfully()
    {
        // Arrange
        var createRequest = new CreateStandardRequest
        {
            Title = "Tag Removal Test",
            Description = "Testing tag removal",
            Category = "Testing",
            Tags = new List<string> { "important", "urgent" }
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/standards", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdStandard = JsonSerializer.Deserialize<StandardResponse>(createdContent, JsonOptions)!;

        // Act
        var response = await _fixture.Client.DeleteAsync($"/api/standards/{createdStandard.Id}/tags/important");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandardResponse>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.DoesNotContain("important", result!.Tags);
        Assert.Contains("urgent", result.Tags);
    }
}
