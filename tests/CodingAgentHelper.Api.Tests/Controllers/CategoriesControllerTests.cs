namespace CodingAgentHelper.Api.Tests.Controllers;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CodingAgentHelper.Api.Models;
using CodingAgentHelper.Api.Tests.Infrastructure;
using Xunit;

[Collection("API Tests")]
public class CategoriesControllerTests
{
    private readonly ApiTestFixture _fixture;

    public CategoriesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOkResult()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CategoryListResponse>(content);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest
        {
            Name = "Performance",
            Description = "Performance optimization standards"
        };

        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/categories", createRequest);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryResponse>(createdContent);

        // Act
        var response = await _fixture.Client.GetAsync($"/api/categories/{createdCategory!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CategoryResponse>(content);
        Assert.NotNull(result);
        Assert.Equal(createdCategory.Id, result!.Id);
        Assert.Equal("Performance", result.Name);
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _fixture.Client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Security",
            Description = "Security and compliance standards"
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/categories", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CategoryResponse>(content);
        Assert.NotNull(result);
        Assert.Equal("Security", result!.Name);
        Assert.Equal("Security and compliance standards", result.Description);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "",
            Description = "Description"
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/api/categories", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Testing",
            Description = "Testing standards"
        };

        // Create first category
        await _fixture.Client.PostAsJsonAsync("/api/categories", request);

        // Act - Try to create duplicate
        var response = await _fixture.Client.PostAsJsonAsync("/api/categories", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest
        {
            Name = "Original",
            Description = "Original description"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/categories", createRequest);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryResponse>(createdContent)!;

        var updateRequest = new UpdateCategoryRequest
        {
            Description = "Updated description"
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"/api/categories/{createdCategory.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CategoryResponse>(content);
        Assert.NotNull(result);
        Assert.Equal("Updated description", result!.Description);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest
        {
            Name = "ToDelete",
            Description = "To be deleted"
        };
        var createResponse = await _fixture.Client.PostAsJsonAsync("/api/categories", createRequest);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryResponse>(createdContent)!;

        // Act
        var response = await _fixture.Client.DeleteAsync($"/api/categories/{createdCategory.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's deleted
        var getResponse = await _fixture.Client.GetAsync($"/api/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _fixture.Client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
