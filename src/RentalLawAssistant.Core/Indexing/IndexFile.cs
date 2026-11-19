using System.Text.Json;

namespace RentalLawAssistant.Core.Indexing;

/// <summary>Reads and writes the index file (JSON, snake_case keys).</summary>
public static class IndexFile
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    /// <summary>Writes to a temporary file first and then replaces the target, so a crash never leaves a half-written index.</summary>
    public static async Task WriteAsync(string path, ArticleIndex index, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(index);

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, index, SerializerOptions, cancellationToken);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }

    /// <exception cref="FileNotFoundException">The index file does not exist (run the ingestion).</exception>
    /// <exception cref="InvalidDataException">The file exists but is not a valid index.</exception>
    public static async Task<ArticleIndex> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Index file not found: {Path.GetFullPath(path)}. Run the ingestion first: dotnet run --project src/RentalLawAssistant.Ingestion",
                path);
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var index = await JsonSerializer.DeserializeAsync<ArticleIndex>(stream, SerializerOptions, cancellationToken);
            return index is { Articles: not null, EmbeddingModel: not null }
                ? index
                : throw new InvalidDataException($"Index file {Path.GetFullPath(path)} is empty or incomplete. Run the ingestion again.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Index file {Path.GetFullPath(path)} is not valid JSON ({exception.Message}). Run the ingestion again.",
                exception);
        }
    }
}
