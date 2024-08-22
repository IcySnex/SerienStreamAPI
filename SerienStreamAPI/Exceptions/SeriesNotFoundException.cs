namespace SerienStreamAPI.Exceptions;

public class SeriesNotFoundException : Exception
{
    public SeriesNotFoundException(
        string title) : base("Could not find any series with given title.")
    {
        this.Title = title;
    }

    public SeriesNotFoundException(
        string title,
        string? message) : base(
            message)
    {
        this.Title = title;
    }

    public SeriesNotFoundException(
        string title,
        string? message,
        Exception? innerException) : base(
            message,
            innerException)
    {
        this.Title = title;
    }


    public string Title { get; }
}