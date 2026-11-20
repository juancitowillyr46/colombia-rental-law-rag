using System.Globalization;
using System.Text;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Answering;

/// <summary>Prompts for the chat model. Written in English; the model is asked to answer in Spanish.</summary>
public static class Prompts
{
    public const string System = """
        You answer questions about Colombian residential rental law (vivienda urbana) for people without legal training.

        Rules:
        - Use ONLY the articles provided in the user message. Each article starts with its id in square brackets, e.g. [cc-1985].
        - Write the answer in Spanish, in 2 to 4 short sentences, in plain language.
        - Every statement must be supported by a provided article. Put the ids of the articles you relied on in citedArticleIds,
          copied exactly as they appear in the brackets. Do not cite anything that was not provided.
        - Answer the exact question asked. The articles must explicitly deal with the subject of the question. Do not extend a
          rule about one subject to a different subject, and do not use facts, figures or amounts that are not written in the
          provided articles. Articles about a related but different subject do not answer the question.
        - Before answering, check who the articles say must pay or act (tenant/arrendatario or landlord/arrendador) and make
          sure your answer says the same.
        - Do not quote long passages; the application shows the article text to the user.
        - If the provided articles do not answer the question, set grounded to false, leave citedArticleIds empty and do not
          invent an answer.
        - Give general information only. Do not give personalized legal advice and do not tell the user what to sign or sue.

        Return JSON with the fields: answer (string), citedArticleIds (array of strings), grounded (boolean).
        """;

    /// <summary>The question plus the retrieved articles, each labelled with the article id the model must cite.</summary>
    public static string BuildUserPrompt(string question, IReadOnlyList<ScoredArticle> articles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentNullException.ThrowIfNull(articles);

        var prompt = new StringBuilder();
        prompt.AppendLine("Articles:");
        prompt.AppendLine();
        foreach (var scored in articles)
        {
            var article = scored.Article;
            prompt.AppendLine(CultureInfo.InvariantCulture, $"[{article.ArticleId}] {article.LawName}, artículo {article.ArticleNumber}");
            prompt.AppendLine(article.Content);
            prompt.AppendLine();
        }

        prompt.AppendLine("Question:");
        prompt.Append(question.Trim());
        return prompt.ToString();
    }
}
