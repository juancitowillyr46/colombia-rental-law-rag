using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;

namespace RentalLawAssistant.Tests.Fakes;

/// <summary>
/// A tiny hand-made index in 2-D: questions pointing along (1, 0) match the "repairs" articles,
/// questions pointing along (-1, 0) match nothing well (similarity below any sensible threshold).
/// </summary>
public static class TestCorpus
{
    public const string GoodQuestion = "¿Quién paga el calentador viejo?";
    public const string UnrelatedQuestion = "¿Cuánto IVA paga un local comercial?";

    public static ArticleIndex Index { get; } = new("bge-m3", 2, DateTimeOffset.UnixEpoch,
    [
        Fragment("cc-1985", "Código Civil", "1985", [1f, 0f], "Texto del 1985 sobre reparaciones necesarias."),
        Fragment("cc-2028", "Código Civil", "2028", [0.9f, 0.3f], "Texto del 2028 sobre vetustez."),
        Fragment("cc-2035", "Código Civil", "2035", [0.95f, 0.1f], "Artículo derogado.", inForce: false),
        Fragment("l820-15#1", "Ley 820 de 2003", "15", [0.2f, 1f], "Primer inciso del 15."),
        Fragment("l820-15#2", "Ley 820 de 2003", "15", [0.1f, 1f], "Segundo inciso del 15."),
    ]);

    public static FakeEmbeddingGenerator Embeddings()
    {
        var generator = new FakeEmbeddingGenerator(dimensions: 2);
        generator.SetVector(GoodQuestion, [1f, 0f]);
        generator.SetVector(UnrelatedQuestion, [-1f, 0f]);
        return generator;
    }

    private static IndexedArticle Fragment(string id, string law, string number, float[] embedding, string content, bool inForce = true) =>
        new(new LawArticle(id, id.Split('-')[0], law, number, content, inForce, $"https://example.org/{id.Split('-')[0]}"), "hash", "bge-m3", embedding);
}
