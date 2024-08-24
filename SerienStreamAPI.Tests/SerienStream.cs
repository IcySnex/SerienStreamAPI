#pragma warning disable IDE1006 // Naming Styles

using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SerienStreamAPI.Client;
using SerienStreamAPI.Models;

namespace SerienStreamAPI.Tests;

public class SerienStream
{
    ILogger<SerienStream> logger;

    SerienStreamClient client;

    [SetUp]
    public void Setup()
    {
        logger = TestData.CreateLogger<SerienStream>();
        client = new(TestData.HostUrl, TestData.Site, TestData.IgnoreCerficiateValidation, TestData.CreateLogger<SerienStreamClient>());
    }


    [Test]
    public void get_any_series()
    {
        Series? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetSeriesAsync(TestData.Title);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }


    [Test]
    public void get_all_episodes_of_any_season_from_any_series()
    {
        Media[]? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetEpisodesAsync(TestData.Title, TestData.Season);
        });
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);

        logger.LogObject(result);
    }

    [Test]
    public void get_all_movies_from_any_series()
    {
        Media[]? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetMoviesAsync(TestData.Title);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }


    [Test]
    public void get_the_video_info_of_any_episode_on_any_season_from_any_series()
    {
        VideoDetails? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetEpisodeVideoInfoAsync(TestData.Title, TestData.Episode, TestData.Season);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }

    [Test]
    public void get_the_video_info_of_any_movie_from_any_series()
    {
        VideoDetails? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetMovieVideoInfoAsync(TestData.Title, TestData.Movie);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }
}