#pragma warning disable IDE1006 // Naming Styles

using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SerienStreamAPI.Client;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace SerienStreamAPI.Tests;

public class Download
{
    ILogger<DownloadClient> logger;
    DownloadClient client;

    [SetUp]
    public void Setup()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        logger = factory.CreateLogger<DownloadClient>();
        client = new(logger);
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
}