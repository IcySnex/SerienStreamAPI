namespace SerienStreamAPI.Exceptions;

public class MovieNotFoundException : Exception
{
    public MovieNotFoundException(
        string title,
        int movie) : base("Could not find any movie with given number on the series.")
    {
        this.Title = title;
        this.Movie = movie;
    }

    public MovieNotFoundException(
        string title,
        int movie,
        string? message) : base(
            message)
    {
        this.Title = title;
        this.Movie = movie;
    }

    public MovieNotFoundException(
        string title,
        int movie,
        string? message,
        Exception? innerException) : base(
            message,
            innerException)
    {
        this.Title = title;
        this.Movie = movie;
    }


    public string Title { get; }

    public int Movie { get; }
}