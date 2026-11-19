namespace RentalLawAssistant.Core.Messages;

/// <summary>Every text shown to the end user, in Spanish, in one place.</summary>
public static class SpanishMessages
{
    public const string Disclaimer = "Información general, no es asesoría legal.";

    public const string NoBasis =
        "No encontré base suficiente en las normas consultadas (Ley 820 de 2003, Código Civil y Decreto 1077 de 2015) " +
        "para responder esa pregunta. Si tu caso es particular, consulta con un abogado o con la oficina de vivienda de tu municipio.";

    public const string QuestionRequired = "Escribe una pregunta.";

    public static string QuestionTooLong(int maxLength) =>
        $"La pregunta es demasiado larga. Usa máximo {maxLength} caracteres.";
}
