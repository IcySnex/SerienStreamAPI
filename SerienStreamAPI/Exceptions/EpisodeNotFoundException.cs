namespace SerienStreamAPI.Exceptions;

public class EpisodeNotFoundException : Exception
{
    public EpisodeNotFoundException(
        string title,
        int season,
        int episode) : base("Could not find any episode with given number of the season on the series.")
    {
        this.Title = title;
        this.Season = season;
        this.Episode = episode;
    }

    public EpisodeNotFoundException(
        string title,
        int season,
        int episode,
        string? message) : base(
            message)
    {
        this.Title = title;
        this.Season = season;
        this.Episode = episode;
    }

    public EpisodeNotFoundException(
        string title,
        int season,
        int episode,
        string? message,
        Exception? innerException) : base(
            message,
            innerException)
    {
        this.Title = title;
        this.Season = season;
        this.Episode = episode;
    }


    public string Title { get; }

    public int Season { get; }

    public int Episode { get; }
}