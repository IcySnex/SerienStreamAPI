#pragma warning disable IDE1006 // Naming Styles

using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SerienStreamAPI.Client;
using SerienStreamAPI.Enums;
using SerienStreamAPI.Exceptions;
using SerienStreamAPI.Models;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SerienStreamAPI.Tests;

public class Example
{
    static VideoStream? SelectDesiredVideoStream(
        VideoStream[] videoStreams,
        Language audioLanguage,
        Language? subtitleLanguage,
        Hoster hoster) =>
        videoStreams.OrderByDescending(stream =>
            (stream.Language.Audio == audioLanguage ? 4 : 0) +
            (stream.Language.Subtitle == subtitleLanguage ? 2 : 0) +
            (stream.Hoster == hoster ? 1 : 0))
            .FirstOrDefault();

    static string MakeSafe(
        string text,
        bool isDirectory = false)
    {
        foreach (char c in isDirectory ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars())
            text = text.Replace(c.ToString(), "");

        return text;
    }


    ILogger<Example> logger;

    SerienStreamClient client;
    DownloadClient downloadClient;

    [SetUp]
    public void Setup()
    {
        logger = TestData.CreateLogger<Example>();

        client = new(TestData.HostUrl, TestData.Site, TestData.IgnoreCerficiateValidation);
        downloadClient = new(TestData.FFmpegLocation, TestData.IgnoreCerficiateValidation);
    }


    [Test]
    public async Task download_everything_of_any_series()
    {
        Series series = await client.GetSeriesAsync(TestData.Title);
        logger.LogInformation("Found Series: {title} - {seasonsCount} Seasons, {moviesInfo} Movies", series.Title, series.SeasonsCount, series.HasMovies ? "Contains" : "No");

        if (false) //series.HasMovies)
        {
            logger.LogInformation("  Starting to download all movies...");

            string downloadDirectory = Path.Combine(TestData.DownloadDirectory, MakeSafe(series.Title), "Movies");
            Directory.CreateDirectory(downloadDirectory);

            Media[] movies = await client.GetMoviesAsync(TestData.Title);
            foreach (Media movie in movies)
            {
                logger.LogInformation("    Preparing movie download: [{index}/{total}] {title}", movie.Number, movies.Length, movie.Title);
                VideoDetails details = await client.GetMovieVideoInfoAsync(series.Title, movie.Number);

                await DownloadMediaAsync(details, downloadDirectory);
            }

            logger.LogInformation("  Finished downloading all movies!");
        }

        for (int i = 1; i < series.SeasonsCount + 1; i++)
        {
            logger.LogInformation("  Starting to download season {number}...", i);

            string downloadDirectory = Path.Combine(TestData.DownloadDirectory, MakeSafe(series.Title), $"Season {i}");
            Directory.CreateDirectory(downloadDirectory);

            Media[] episodes = await client.GetEpisodesAsync(TestData.Title, i);
            foreach (Media episode in episodes)
            {
                logger.LogInformation("    Preparing episode download: [{index}/{total}] {title}", episode.Number, episodes.Length, episode.Title);
                VideoDetails details = await client.GetEpisodeVideoInfoAsync(series.Title, episode.Number, i);

                await DownloadMediaAsync(details, downloadDirectory);
            }

            logger.LogInformation("  Finished downloading season {number}!", i);
        }
    }


    async Task DownloadMediaAsync(
        VideoDetails details,
        string downloadDirectory)
    {
        VideoStream? bestStream = SelectDesiredVideoStream(details.Streams, TestData.DesiredAudioLanguage, TestData.DesiredSubtitleLanguage, TestData.DesiredHoster);
        if (bestStream is null)
        {
            logger.LogWarning("    Media video details do not contain any streams. Skipping...");
            return;
        }

        string? streamUrl = bestStream.Hoster switch
        {
            Hoster.VOE => await downloadClient.GetVoeStreamUrlAsync(bestStream.VideoUrl),
            Hoster.Streamtape => await downloadClient.GetStreamtapeStreamUrlAsync(bestStream.VideoUrl),
            //Hoster.Doodstream => await downloadClient.GetDoodstreamStreamUrlAsync(bestStream.VideoUrl),
            Hoster.Vidoza => await downloadClient.GetVidozaStreamUrlAsync(bestStream.VideoUrl),
            _ => null
        };
        if (streamUrl is null)
        {
            logger.LogWarning("    Failed to get media stream url. Skipping...");
            return;
        }

        logger.LogWarning("    Starting media download: Audio: {audioLanguage}, Subtitle: {subtitleLanguage}, Hoster: {hoster}", bestStream.Language.Audio, bestStream.Language.Subtitle, bestStream.Hoster);
        string filePath = Path.Combine(downloadDirectory, $"{details.Number:D2} {MakeSafe(string.IsNullOrEmpty(details.Title) ? details.OriginalTitle : details.Title)}.mp4");
        
        await downloadClient.DownloadAsync(streamUrl, filePath, null, new Progress<EncodingProgress>(progress =>
            logger.LogInformation("      Downloading media... {timeElapsed}", progress.TimeElapsed)));
    }
}
