#pragma warning disable IDE1006 // Naming Styles

using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SerienStreamAPI.Client;
using SerienStreamAPI.Models;

namespace SerienStreamAPI.Tests;

public class Download
{
    ILogger<Download> logger;

    DownloadClient client;

    [SetUp]
    public void Setup()
    {
        logger = TestData.CreateLogger<Download>();
        client = new(TestData.FFmpegLocation, TestData.IgnoreCerficiateValidation, TestData.CreateLogger<DownloadClient>());
    }


    [Test]
    public void get_voe_stream_url()
    {
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetVoeStreamUrlAsync(TestData.VoeVideoUrl);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }

    [Test]
    public void get_streamtape_stream_url()
    {
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetStreamtapeStreamUrlAsync(TestData.StreamtapeVideoUrl);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }

    [Test]
    public void get_doodstream_stream_url()
    {
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetDoodstreamStreamUrlAsync(TestData.DoodstreamVideoUrl);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }

    [Test]
    public void get_vidoza_stream_url()
    {
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetVidozaStreamUrlAsync(TestData.VidozaVideoUrl);
        });
        Assert.That(result, Is.Not.Null);

        logger.LogObject(result);
    }


    [Test]
    public void download_stream_to_file_path()
    {
        Assert.DoesNotThrowAsync(async () =>
        {
            await client.DownloadAsync(TestData.StreamUrl, TestData.FilePath, TestData.Headers, new Progress<EncodingProgress>(progress =>
                logger.LogInformation("Progres:\n\tFramesProcessed: {framesProcessed}\n\tFps: {fps}\n\tQuality: {quality}\n\tOutputFileSizeKb: {outputFileSizeKb}\n\tTimeElapsed: {timeElapsed}\n\tBitrateKbps: {bitrateKbps}\n\tSpeedMultiplier: {speedMultiplier}",
                    progress.FramesProcessed, progress.Fps, progress.Quality, progress.OutputFileSizeKb, progress.TimeElapsed, progress.BitrateKbps, progress.SpeedMultiplier)));
        });

        logger.LogInformation("Result:\n\tDownloaded video successfully.");
    }
}