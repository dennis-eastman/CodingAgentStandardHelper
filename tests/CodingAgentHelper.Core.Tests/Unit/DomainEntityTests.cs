namespace CodingAgentHelper.Core.Tests.Unit;

using Builders;
using Domain.Entities;
using Xunit;

public class StandardTests
{
    [Fact]
    public void StandardConstructor_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        const string title = "Test Standard";
        const string description = "Test Description";
        const string category = "Testing";

        // Act
        var standard = new Standard(title, description, category);

        // Assert
        Assert.NotEqual(Guid.Empty, standard.Id);
        Assert.Equal(title, standard.Title);
        Assert.Equal(description, standard.Description);
        Assert.Equal(category, standard.Category);
        Assert.Equal(StandardStatus.Active, standard.Status);
        Assert.Equal(StandardPriority.Medium, standard.Priority);
        Assert.Empty(standard.Tags);
        Assert.NotEqual(default, standard.CreatedAt);
        Assert.NotEqual(default, standard.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void StandardConstructor_WithInvalidTitle_ThrowsException(string invalidTitle)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Standard(invalidTitle, "Description", "Category"));
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_UpdatesSuccessfully()
    {
        // Arrange
        var standard = new StandardBuilder()
            .WithStatus(StandardStatus.Active)
            .Build();

        var originalUpdatedAt = standard.UpdatedAt;

        // Act
        standard.UpdateStatus(StandardStatus.Archived);

        // Assert
        Assert.Equal(StandardStatus.Archived, standard.Status);
        Assert.True(standard.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void AddTag_WithValidTag_AddsSuccessfully()
    {
        // Arrange
        var standard = new StandardBuilder().Build();

        // Act
        standard.AddTag("important");

        // Assert
        Assert.Contains("important", standard.Tags);
    }

    [Fact]
    public void AddTag_WithDuplicateTag_DoesNotDuplicate()
    {
        // Arrange
        var standard = new StandardBuilder().Build();

        // Act
        standard.AddTag("important");
        standard.AddTag("important");

        // Assert
        Assert.Single(standard.Tags);
    }

    [Fact]
    public void RemoveTag_WithExistingTag_RemovesSuccessfully()
    {
        // Arrange
        var standard = new StandardBuilder()
            .WithTag("important")
            .Build();

        // Act
        standard.RemoveTag("important");

        // Assert
        Assert.Empty(standard.Tags);
    }
}

public class CategoryTests
{
    [Fact]
    public void CategoryConstructor_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        const string name = "Test Category";
        const string description = "Test Description";

        // Act
        var category = new Category(name, description);

        // Assert
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(name, category.Name);
        Assert.Equal(description, category.Description);
        Assert.Equal(0, category.StandardCount);
        Assert.NotEqual(default, category.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CategoryConstructor_WithInvalidName_ThrowsException(string invalidName)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Category(invalidName, "Description"));
    }

    [Fact]
    public void UpdateDescription_WithNewDescription_UpdatesSuccessfully()
    {
        // Arrange
        var category = new Category("Category", "Original Description");
        const string newDescription = "Updated Description";

        // Act
        category.UpdateDescription(newDescription);

        // Assert
        Assert.Equal(newDescription, category.Description);
    }
}
