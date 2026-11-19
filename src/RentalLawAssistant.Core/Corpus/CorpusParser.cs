using System.Text;
using System.Text.RegularExpressions;

namespace RentalLawAssistant.Core.Corpus;

/// <summary>
/// Parses corpus Markdown files (YAML-like front matter + one "## Artículo N" heading per article)
/// into <see cref="LawArticle"/> fragments. Fails with <see cref="CorpusFormatException"/> on any
/// format problem, so a broken corpus never reaches the index silently.
/// </summary>
public sealed partial class CorpusParser
{
    public const int DefaultMaxChunkLength = 1500;
    public const string SourcesFileName = "SOURCES.md";

    private const string RepealedStatus = "repealed";
    private const string FrontMatterDelimiter = "---";
    private const string ParagraphSeparator = "\n\n";

    private readonly int _maxChunkLength;

    public CorpusParser(int maxChunkLength = DefaultMaxChunkLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxChunkLength);
        _maxChunkLength = maxChunkLength;
    }

    /// <summary>Parses every <c>*.md</c> file in <paramref name="directory"/> except SOURCES.md.</summary>
    public IReadOnlyList<LawArticle> ParseDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        if (!Directory.Exists(directory))
        {
            throw new CorpusFormatException($"Corpus directory not found: {Path.GetFullPath(directory)}");
        }

        var files = Directory.EnumerateFiles(directory, "*.md")
            .Where(file => !string.Equals(Path.GetFileName(file), SourcesFileName, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToList();
        if (files.Count == 0)
        {
            throw new CorpusFormatException($"No corpus files (*.md) found in {Path.GetFullPath(directory)}");
        }

        var articles = new List<LawArticle>();
        var fileById = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            foreach (var article in Parse(File.ReadAllText(file), fileName))
            {
                if (!fileById.TryAdd(article.Id, fileName))
                {
                    throw new CorpusFormatException(
                        $"Duplicate article id '{article.Id}' in {fileById[article.Id]} and {fileName}.");
                }

                articles.Add(article);
            }
        }

        return articles;
    }

    /// <summary>Parses one corpus file. <paramref name="sourceName"/> is only used in error messages.</summary>
    public IReadOnlyList<LawArticle> Parse(string markdown, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        var lines = markdown.ReplaceLineEndings("\n").Split('\n');
        var (frontMatter, bodyStart) = ParseFrontMatter(lines, sourceName);
        var law = new LawInfo(
            Id: RequireKey(frontMatter, "law_id", sourceName),
            Name: RequireKey(frontMatter, "law_name", sourceName),
            SourceUrl: RequireKey(frontMatter, "source_url", sourceName));

        var articles = new List<LawArticle>();
        var seenNumbers = new HashSet<string>(StringComparer.Ordinal);
        string? number = null;
        var headingLine = 0;
        var body = new List<string>();

        for (var i = bodyStart; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                var match = ArticleHeadingRegex().Match(line);
                if (!match.Success)
                {
                    throw new CorpusFormatException(
                        $"{sourceName}, line {i + 1}: invalid heading '{line.Trim()}'. Expected '## Artículo <number>'.");
                }

                AddArticle();
                number = match.Groups["number"].Value;
                headingLine = i + 1;
                if (!seenNumbers.Add(number))
                {
                    throw new CorpusFormatException($"{sourceName}, line {headingLine}: article {number} appears twice.");
                }

                body.Clear();
                continue;
            }

            if (number is null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    throw new CorpusFormatException(
                        $"{sourceName}, line {i + 1}: text found before the first '## Artículo' heading.");
                }

                continue;
            }

            body.Add(line);
        }

        AddArticle();
        if (articles.Count == 0)
        {
            throw new CorpusFormatException($"{sourceName}: no '## Artículo' headings found.");
        }

        return articles;

        void AddArticle()
        {
            if (number is not null)
            {
                articles.AddRange(BuildFragments(law, number, string.Join('\n', body), sourceName, headingLine));
            }
        }
    }

    private List<LawArticle> BuildFragments(LawInfo law, string number, string rawBody, string sourceName, int headingLine)
    {
        var inForce = true;
        foreach (Match status in StatusCommentRegex().Matches(rawBody))
        {
            var value = status.Groups["value"].Value;
            if (!string.Equals(value, RepealedStatus, StringComparison.Ordinal))
            {
                throw new CorpusFormatException(
                    $"{sourceName}, article {number} (line {headingLine}): unknown status '{value}'. Only '{RepealedStatus}' is supported.");
            }

            inForce = false;
        }

        var content = NormalizeContent(HtmlCommentRegex().Replace(rawBody, string.Empty));
        if (content.Length == 0)
        {
            throw new CorpusFormatException($"{sourceName}, article {number} (line {headingLine}) has no content.");
        }

        var articleId = $"{law.Id}-{number}";
        var chunks = SplitIntoChunks(content);
        return
        [
            .. chunks.Select((chunk, i) => new LawArticle(
                chunks.Count == 1 ? articleId : $"{articleId}#{i + 1}",
                law.Id,
                law.Name,
                number,
                chunk,
                inForce,
                law.SourceUrl)),
        ];
    }

    /// <summary>
    /// Keeps short articles whole. Longer ones are packed paragraph by paragraph into chunks of at most
    /// the configured length; a single paragraph is never cut, even if it is longer than the limit.
    /// </summary>
    private List<string> SplitIntoChunks(string content)
    {
        if (content.Length <= _maxChunkLength)
        {
            return [content];
        }

        var chunks = new List<string>();
        var current = new StringBuilder();
        foreach (var paragraph in content.Split(ParagraphSeparator))
        {
            if (current.Length > 0 && current.Length + ParagraphSeparator.Length + paragraph.Length > _maxChunkLength)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0)
            {
                current.Append(ParagraphSeparator);
            }

            current.Append(paragraph);
        }

        chunks.Add(current.ToString());
        return chunks;
    }

    private static string NormalizeContent(string text)
    {
        var lines = text.Split('\n').Select(line => line.TrimEnd());
        return ExtraBlankLinesRegex().Replace(string.Join('\n', lines), ParagraphSeparator).Trim();
    }

    private static (Dictionary<string, string> Values, int BodyStart) ParseFrontMatter(string[] lines, string sourceName)
    {
        if (lines.Length == 0 || lines[0].Trim() != FrontMatterDelimiter)
        {
            throw new CorpusFormatException($"{sourceName}: the file must start with a '---' front matter block.");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line == FrontMatterDelimiter)
            {
                return (values, i + 1);
            }

            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                break;
            }

            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                throw new CorpusFormatException($"{sourceName}, line {i + 1}: front matter lines must be 'key: value'.");
            }

            values[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        throw new CorpusFormatException($"{sourceName}: the front matter block is not closed with '---'.");
    }

    private static string RequireKey(Dictionary<string, string> frontMatter, string key, string sourceName) =>
        frontMatter.TryGetValue(key, out var value) && value.Length > 0
            ? value
            : throw new CorpusFormatException($"{sourceName}: front matter key '{key}' is missing or empty.");

    [GeneratedRegex(@"^##\s+Art[ií]culo\s+(?<number>[0-9][0-9A-Za-z.\-]*)\s*$")]
    private static partial Regex ArticleHeadingRegex();

    [GeneratedRegex(@"<!--\s*status:\s*(?<value>\S+)\s*-->")]
    private static partial Regex StatusCommentRegex();

    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExtraBlankLinesRegex();

    private sealed record LawInfo(string Id, string Name, string SourceUrl);
}
