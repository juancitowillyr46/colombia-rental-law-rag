namespace RentalLawAssistant.Core.Corpus;

/// <summary>The corpus files do not follow the expected format; the message says where and why.</summary>
public sealed class CorpusFormatException : Exception
{
    public CorpusFormatException()
    {
    }

    public CorpusFormatException(string message)
        : base(message)
    {
    }

    public CorpusFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
