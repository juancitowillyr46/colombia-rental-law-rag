using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;

namespace RentalLawAssistant.Tests.Fakes;

/// <summary>
/// Deterministic stand-in for bge-m3: the same text always gets the same vector, and every call is
/// recorded so tests can check what was (and was not) sent to the model. No Ollama needed.
/// </summary>
public sealed class FakeEmbeddingGenerator(int dimensions = 8) : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly Dictionary<string, float[]> _fixedVectors = new(StringComparer.Ordinal);

    public List<string> Inputs { get; } = [];

    public int Calls { get; private set; }

    /// <summary>Forces the vector returned for an exact input text.</summary>
    public void SetVector(string input, float[] vector) => _fixedVectors[input] = vector;

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputs = values.ToList();
        Calls++;
        Inputs.AddRange(inputs);
        var embeddings = inputs.Select(input => new Embedding<float>(VectorFor(input)));
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    private float[] VectorFor(string input)
    {
        if (_fixedVectors.TryGetValue(input, out var vector))
        {
            return vector;
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return [.. Enumerable.Range(0, dimensions).Select(i => (hash[i % hash.Length] / 255f) - 0.5f)];
    }
}
