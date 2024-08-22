namespace SerienStreamAPI.Exceptions;

public class SeasonNotFoundException : Exception
{
    public SeasonNotFoundException(
        string title,
        int season) : base("Could not find any season with given number on the series.")
    {
        this.Title = title;
        this.Season = season;
    }

    public SeasonNotFoundException(
        string title,
        int season,
        string? message) : base(
            message)
    {
        this.Title = title;
        this.Season = season;
    }

    public SeasonNotFoundException(
        string title,
        int season,
        string? message,
        Exception? innerException) : base(
            message,
            innerException)
    {
        this.Title = title;
        this.Season = season;
    }


    public string Title { get; }

    public int Season { get; }
}