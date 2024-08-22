namespace SerienStreamAPI.Exceptions;

public class UrlExtractionFailedException : Exception
{
    public UrlExtractionFailedException(
        string sourceUrl) : base("Failed to extract url from source.")
    {
        this.SourceUrl = sourceUrl;
    }

    public UrlExtractionFailedException(
        string sourceUrl,
        string? message) : base(
            message)
    {
        this.SourceUrl = sourceUrl;
    }

    public UrlExtractionFailedException(
        string sourceUrl,
        string? message,
        Exception? innerException) : base(
            message,
            innerException)
    {
        this.SourceUrl = sourceUrl;
    }


    public string SourceUrl { get; }
}