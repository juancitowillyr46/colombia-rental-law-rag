namespace RentalLawAssistant.Core.Corpus;

/// <summary>
/// One retrievable fragment of the corpus. Usually a whole article; long articles are split by
/// paragraphs into chunks whose <see cref="Id"/> carries a suffix ("l820-15#2").
/// </summary>
/// <param name="Id">Unique fragment id: "cc-1985", or "l820-15#2" for a chunk.</param>
/// <param name="LawId">Short law id from the front matter: "cc", "l820", "d1077".</param>
/// <param name="LawName">Display name of the law: "Código Civil".</param>
/// <param name="ArticleNumber">Article number as written in the heading: "1985", "2.2.6.1.1.10".</param>
/// <param name="Content">Exact legal text, without HTML comments.</param>
/// <param name="InForce">False when the article is marked <c>&lt;!-- status: repealed --&gt;</c>.</param>
/// <param name="SourceUrl">Official source of the law.</param>
public sealed record LawArticle(
    string Id,
    string LawId,
    string LawName,
    string ArticleNumber,
    string Content,
    bool InForce,
    string SourceUrl)
{
    /// <summary>Id of the whole article ("l820-15"), the same for all its chunks. Citations point here.</summary>
    public string ArticleId => $"{LawId}-{ArticleNumber}";
}
