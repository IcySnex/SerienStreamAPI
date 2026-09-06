using System.Text.Encodings.Web;
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
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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


    public static readonly string HostUrl = "http://186.2.175.5/";

    public static readonly string Site = "serie";

    public static readonly bool IgnoreCertificateValidation = true;

    public static readonly string FFmpegLocation = @"ffmpeg";


    public static readonly string Title = "Seishun Buta Yarou wa Bunny Girl Senpai no Yume o Minai";

    public static readonly int Season = 1;

    public static readonly int Episode = 5;

    public static readonly int Movie = 1;


    public static readonly string RedirectId = "2531389";


    public static readonly string VoeVideoUrl = "http://186.2.175.5/r?t=eyJpdiI6Im5DWERTeEVUb1o2NkkvS25tZ3lzUmc9PSIsInZhbHVlIjoiQ2tlLzJOUEl5ZGUxSU5LdkVZZHZvTFZrZXRHbVhScUR4SW81WkxvLzZtTXRrYWp4eHRRNXVjQ2FuY2tkRTVZWS9TbExoQjVWRkZMZUoxd2VlSHBTSFE9PSIsIm1hYyI6ImNlZTQwYjg0YzM5NGI4OTBiNzM3YzYyZjdkMzgzODdmZGU4NmIwYWViNGQ5MGNmYzg0ZjBjM2Q3MjhiZThjMTQiLCJ0YWciOiIifQ%3D%3D";

    public static readonly string StreamtapeVideoUrl = "https://streamtape.com/v/lARoV4B6vps7kbg/unknown_replay_2025.05.29-11.32.mp4";

    public static readonly string DoodstreamVideoUrl = "https://vide0.net/e/whjxr0pbrqj1";

    public static readonly string VidozaVideoUrl = "https://186.2.175.5/redirect/17839891";


    public static readonly string StreamUrl = "https://cdn-lkvxgheee2fzfh03.edgeon-bandwidth.com/engine/hls2/01/00645/d8vqxu6cbq5j_,n,.urlset/master.m3u8?t=GXiyHcbfL9y6790J1-4AAMOM5VTf_vUIQj_gu9L65DQ&s=1770749263&e=14400&f=3225660&node=n8EHxmjUCaQI0GMbkyuEnOHLVaFEQYXrRRHc++IWY5Q=&i=169.150&sp=2500&asn=212238&q=n&rq=jUHMqelEbJhMNiLnCF9pK5zaxXehBohUKAnG9al9";
        
    public static readonly string FilePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/test.mp4";

    public static readonly (string key, string value)[]? Headers = [("Referer", DoodstreamVideoUrl)]; //[("Referer", DoodstreamVideoUrl)]; // Header required when downloading stream from doodstream


    public static readonly string DownloadDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static readonly Language DesiredAudioLanguage = Language.German;

    public static readonly Language? DesiredSubtitleLanguage = null;

    public static readonly Hoster DesiredHoster = Hoster.VOE;
}
