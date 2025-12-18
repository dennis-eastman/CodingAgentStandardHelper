namespace CodingAgentHelper.Core.Infrastructure.VectorStore;

/// <summary>
/// Interface for vector embedding operations
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<Dictionary<string, float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
}

/// <summary>
/// Simple embedding service using mock embeddings for Phase 1
/// Will be replaced with actual ML model (e.g., Sentence Transformers) in Phase 2
/// </summary>
public class MockEmbeddingService : IEmbeddingService
{
    private readonly Random _random = new(42); // Deterministic for testing
    private const int EmbeddingDimension = 384;

    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty", nameof(text));

        var embedding = GenerateEmbedding(text);
        return Task.FromResult(embedding);
    }

    public Task<Dictionary<string, float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, float[]>();
        foreach (var text in texts)
        {
            result[text] = GenerateEmbedding(text);
        }
        return Task.FromResult(result);
    }

    private float[] GenerateEmbedding(string text)
    {
        // Generate deterministic embedding based on text hash
        var embedding = new float[EmbeddingDimension];
        var hash = text.GetHashCode();
        
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            var seed = (hash ^ i).GetHashCode();
            var rng = new Random(seed);
            embedding[i] = (float)rng.NextDouble() * 2 - 1; // Range [-1, 1]
        }

        // Normalize to unit length
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }

        return embedding;
    }
}
