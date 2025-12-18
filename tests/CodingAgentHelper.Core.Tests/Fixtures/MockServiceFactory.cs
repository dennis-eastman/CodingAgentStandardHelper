namespace CodingAgentHelper.Core.Tests.Fixtures;

using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.VectorStore;
using Moq;

/// <summary>
/// Factory for creating mock service dependencies
/// </summary>
public class MockServiceFactory
{
    public static Mock<IChromaClient> CreateMockChromaClient()
    {
        var mock = new Mock<IChromaClient>();
        
        // Default behavior: operations succeed
        mock.Setup(c => c.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mock.Setup(c => c.CollectionExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mock.Setup(c => c.CreateCollectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(c => c.AddEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<VectorEmbedding>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(c => c.SearchAsync(It.IsAny<string>(), It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VectorSearchResult>());

        return mock;
    }

    public static Mock<IEmbeddingService> CreateMockEmbeddingService()
    {
        var mock = new Mock<IEmbeddingService>();

        mock.Setup(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string text, CancellationToken _) =>
            {
                // Return deterministic embedding
                var embedding = new float[384];
                var hash = text.GetHashCode();
                for (int i = 0; i < 384; i++)
                {
                    embedding[i] = (hash ^ i) % 100 / 100f;
                }
                return embedding;
            });

        return mock;
    }

    public static Mock<IStandardRepository> CreateMockStandardRepository()
    {
        var mock = new Mock<IStandardRepository>();
        
        mock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Standard>());

        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Standard?)null);

        return mock;
    }

    public static Mock<ICategoryRepository> CreateMockCategoryRepository()
    {
        var mock = new Mock<ICategoryRepository>();

        mock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        return mock;
    }
}
