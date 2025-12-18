namespace CodingAgentHelper.Core.Infrastructure.VectorStore;

/// <summary>
/// Configuration for Chroma vector store connection
/// </summary>
public class ChromaConfiguration
{
    public string Host { get; set; } = "http://localhost";
    public int Port { get; set; } = 8601;
    public string CollectionNamePrefix { get; set; } = "cah_";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;

    public string GetConnectionUrl()
    {
        return $"{Host}:{Port}";
    }

    public string GetCollectionName(string collectionType)
    {
        return $"{CollectionNamePrefix}{collectionType}";
    }

    public TimeSpan GetTimeout()
    {
        return TimeSpan.FromSeconds(TimeoutSeconds);
    }
}

/// <summary>
/// Exception thrown when Chroma vector store operations fail
/// </summary>
public class ChromaException : Exception
{
    public ChromaException(string message) : base(message)
    {
    }

    public ChromaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Data transfer object for vector embeddings
/// </summary>
public class VectorEmbedding
{
    public string Id { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? Document { get; set; }
}

/// <summary>
/// Data transfer object for vector search results
/// </summary>
public class VectorSearchResult
{
    public string Id { get; set; } = string.Empty;
    public float Distance { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? Document { get; set; }
}
