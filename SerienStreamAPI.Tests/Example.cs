#pragma warning disable IDE1006 // Naming Styles

using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SerienStreamAPI.Client;
using SerienStreamAPI.Enums;
using SerienStreamAPI.Models;

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
        string text)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            text = text.Replace(c.ToString(), "");

        return text;
    }

    static int ToInt32(
        bool value) =>
        value ? 1 : 0;


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

        for (int i = ToInt32(!series.HasMovies); i < series.SeasonsCount + ToInt32(series.HasMovies); i++)
        {
            logger.LogInformation("  Starting to download season {number}...", i);

            string downloadDirectory = Path.Combine(TestData.DownloadDirectory, MakeSafe(series.Title), $"Season {i}");
            Directory.CreateDirectory(downloadDirectory);

            Media[] episodes = await client.GetEpisodesAsync(TestData.Title, i);
            foreach (Media episode in episodes)
            {
                logger.LogInformation("    Preparing episode download: [{index}/{total}] {title}", episode.Number, episodes.Length, episode.Title);
                VideoDetails details = await client.GetEpisodeVideoInfoAsync(series.Title, episode.Number, i);

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
                    Hoster.Doodstream => await downloadClient.GetDoodstreamStreamUrlAsync(bestStream.VideoUrl),
                    Hoster.Vidoza => await downloadClient.GetVidozaStreamUrlAsync(bestStream.VideoUrl),
                    _ => null
                };
                if (streamUrl is null)
                {
                    logger.LogWarning("    Failed to get media stream url. Skipping...");
                    return;
                }

                string fileName = $"{MakeSafe(series.Title).Replace(' ', '.')}.S{i:D2}E{details.Number:D2}.{bestStream.Language.Audio}-{bestStream.Hoster}.mp4";
                
                logger.LogInformation("    Starting media download: {fileName}", fileName);
                await downloadClient.DownloadAsync(streamUrl, Path.Combine(downloadDirectory, fileName), null, new Progress<EncodingProgress>(progress =>
                    logger.LogInformation("      Downloading media... Time: {timeElapsed}, Speed: {speed}x", progress.TimeElapsed.ToString(@"hh\:mm\:ss"), progress.SpeedMultiplier)));
            }

            logger.LogInformation("  Finished downloading season {number}!", i);
        }
    }
}
