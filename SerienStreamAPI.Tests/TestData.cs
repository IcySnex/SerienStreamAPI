using Microsoft.Extensions.Logging;
using SerienStreamAPI.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerienStreamAPI.Tests;

public static class TestData
{
    static TestData()
    {
        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        serializerOptions = new()
        {
            WriteIndented = true
        };
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }


    static readonly ILoggerFactory loggerFactory;

    public static ILogger<T> CreateLogger<T>() =>
        loggerFactory.CreateLogger<T>();


    static readonly JsonSerializerOptions serializerOptions;

    public static void LogObject(
        this ILogger logger,
        object @object,
        string message = "Result",
        LogLevel level = LogLevel.Information) =>
        logger.Log(level, "\n{message}:\n\t{readableResults}", message, JsonSerializer.Serialize(@object, serializerOptions));


    public static readonly string HostUrl = "https://aniworld.to/";

    public static readonly string Site = "anime";

    public static readonly bool IgnoreCerficiateValidation = true;

    public static readonly string FFmpegLocation = @"C:\Program Files\FFmpeg\FFmpeg.exe";


    public static readonly string Title = "tonikawa-over-the-moon-for-you";

    public static readonly int Season = 1;

    public static readonly int Episode = 5;

    public static readonly int Movie = 1;


    public static readonly string RedirectId = "2531389";


    public static readonly string VoeVideoUrl = "https://186.2.175.5/redirect/14141709";

    public static readonly string StreamtapeVideoUrl = "https://streamtape.com/v/wzP4qXZRvrIe21";

    public static readonly string DoodstreamVideoUrl = "https://dood.li/e/dp1qdu1v6w1r";

    public static readonly string VidozaVideoUrl = "https://videzz.net/embed-rymjwbo2btf8.html";


    public static readonly string StreamUrl = "https://delivery-node-q3y2c0zkvnd4gnun.voe-network.net/engine/hls2-c/01/11779/ofaw2awtwixx_,n,.urlset/master.m3u8?t=-c4gLZxCvEiVvx7Fgk1SSz9RAgBpsb2Klc5ID0-CfDk&s=1725048286&e=14400&f=58897448&node=delivery-node-tyrpwrfsotxcpbf4.voe-network.net&i=37.201&sp=2500&asn=3209";

    public static readonly string FilePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\test.mp4";

    public static readonly (string key, string value)[]? Headers = null; //[("Referer", DoodstreamVideoUrl)]; // Header requirered when downloading stream from doodstream


    public static readonly string DownloadDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static readonly Language DesiredAudioLanguage = Language.German;

    public static readonly Language? DesiredSubtitleLanguage = null;

    public static readonly Hoster DesiredHoster = Hoster.VOE;
}