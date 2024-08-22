namespace SerienStreamAPI.Models;

public class Series(
    string title,
    string description,
    string bannerUrl,
    int yearStart,
    int? yearEnd,
    string[] directors,
    string[] actors,
    string[] creators,
    string[] countriesOfOrigin,
    string[] genres,
    int ageRating,
    Rating rating,
    string? imdbUrl,
    string? trailerUrl,
    bool hasMovies,
    int seasonsCount)
{
    public string Title { get; } = title;

    public string Description { get; } = description;

    public string BannerUrl { get; } = bannerUrl;

    public int YearStart { get; } = yearStart;

    public int? YearEnd { get; } = yearEnd;

    public string[] Directors { get; } = directors;

    public string[] Actors { get; } = actors;

    public string[] Creators { get; } = creators;

    public string[] CountriesOfOrigin { get; } = countriesOfOrigin;

    public string[] Genres { get; } = genres;

    public int AgeRating { get; } = ageRating;

    public Rating Rating { get; } = rating;

    public string? ImdbUrl { get; } = imdbUrl;

    public string? TrailerUrl { get; } = trailerUrl;

    public bool HasMovies { get; } = hasMovies;

    public int SeasonsCount { get; } = seasonsCount;
}