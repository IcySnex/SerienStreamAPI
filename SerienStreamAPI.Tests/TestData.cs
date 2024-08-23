﻿using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerienStreamAPI.Tests;

public static class TestData
{
    static TestData()
    {
        SerializerOptions = new()
        {
            WriteIndented = true
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }


    static readonly JsonSerializerOptions SerializerOptions;

    public static void LogObject(
        this ILogger logger,
        object @object,
        string message = "Result",
        LogLevel level = LogLevel.Information) =>
        logger.Log(level, "\n{message}:\n\t{readableResults}", message, JsonSerializer.Serialize(@object, SerializerOptions));

    public static ILogger<T> CreateLogger<T>() =>
        LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        }).CreateLogger<T>();


    public const string HostUrl = "https://aniworld.to/";

    public const string Site = "anime";

    public const bool IgnoreCerficiateValidation = true;

    public const string FFmpegLocation = @"C:\Program Files\FFmpeg\FFmpeg.exe";


    public const string Title = "Rascal Does Not Dream of Bunny Girl Senpai";

    public const int Season = 1;

    public const int Episode = 5;

    public const int Movie = 1;


    public const string RedirectId = "2531389";


    public const string VoeVideoUrl = "https://donaldlineelse.com/e/bxgsapjagnq0";

    public const string StreamtapeVideoUrl = "https://streamtape.com/v/wzP4qXZRvrIe21";

    public const string DoodstreamVideoUrl = "https://dood.li/e/dp1qdu1v6w1r";

    public const string VidozaVideoUrl = "https://videzz.net/embed-rymjwbo2btf8.html";


    public const string StreamUrl = "https://streamtape.com/get_video?id=wzP4qXZRvrIe21&expires=1724489297&ip=FHqsDRASKxSHDN&token=Sd_8Bo-RCgzZ&token=Sd_8Bo-RCgdE&dl=1s";

    public static readonly string FilePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\test.mp4";

    public static readonly (string key, string value)[]? Headers = null; //[("Referer", DoodstreamVideoUrl)]; // Header requirered when downloading stream from doodstream
}