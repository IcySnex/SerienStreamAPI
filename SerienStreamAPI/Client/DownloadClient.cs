using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SerienStreamAPI.Internal;
using System.Text.RegularExpressions;
using System.Text;
using SerienStreamAPI.Exceptions;
using System.Diagnostics;
using SerienStreamAPI.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SerienStreamAPI.Client;

public partial class DownloadClient
{
    [GeneratedRegex(@"window\.location\.href\s*=\s*'([^']*)'")]
    private static partial Regex VoeVideoRedirectRegex();

    [GeneratedRegex(@"'hls':\s*'(.*?)'")]
    private static partial Regex VoeStreamUrlRegex();
    
    [GeneratedRegex(@"let\s+\w+\s*=\s*'(.*?)';")]
    private static partial Regex VoeStreamDataRegex();

    [GeneratedRegex("document\\.getElementById\\('norobotlink'\\)\\.innerHTML = (.+);")]
    private static partial Regex StreamtapeNoRobotRegex();

    [GeneratedRegex("token=([^&']+)")]
    private static partial Regex StreamtapeTokenRegex();
    
    [GeneratedRegex(@"/pass_md5/([^/]+/[^']+)")]
    private static partial Regex DoodstreamPassMd5Regex();
    
    [GeneratedRegex(@"frame=\s*(\d+)\s+fps=\s*([\d.]+)\s*q=\s*(-?[\d.]+)\s+size=\s*(\d+)\s*kB\s+time=\s*([\d:.]+)\s+bitrate=\s*([\d.]+)kbits/s(?:\s+[\w=]+)*\s+speed=\s*([\d.]+)x")]
    private static partial Regex FFmpegEncodingProgressRegex();


    private static readonly Random random = new();
    private static readonly string randomCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static string RandomString(
        int lenght = 10)
    {
        string result = "";
        for (int i = 0; i < lenght; i++)
        {
            int index = random.Next(randomCharacters.Length);
            result += randomCharacters[index];
        }

        return result;
    }


    readonly string ffmpegLocation;
    readonly ILogger<DownloadClient>? logger;

    readonly RequestHelper requestHelper;

    public DownloadClient(
        string ffmpegLocation,
        bool ignoreCerficiateValidation = false,
        ILogger<DownloadClient>? logger = null)
    {
        this.ffmpegLocation = ffmpegLocation;
        this.logger = logger;

        this.requestHelper = new(ignoreCerficiateValidation, logger);

        logger?.LogInformation("[DownloadClient-.ctor] DownloadClient has been inizialized.");
    }


    async Task<HtmlNode> GetHtmlRootAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SerienStreamClient-GetHtmlRootAsync] Getting HTML document: {url}...", url);
        string webContent = await requestHelper.GetAndValidateAsync(url, null, null, cancellationToken);

        HtmlDocument document = new();
        document.LoadHtml(webContent);

        return document.DocumentNode;
    }

    Process CreateEncoder(
        string streamUrl,
        string filePath,
        (string key, string value)[]? headers = null,
        DataReceivedEventHandler? onDataReceived = null)
    {
        logger?.LogInformation("[DownloadClient-CreateEncoder] Creating FFmpeg encoder...");
        StringBuilder builder = new();

        if (headers is not null)
            builder.Append($"-headers \"{string.Join(@"\r\n", headers.Select(header => $"{header.key}: {header.value}"))}\" ");
        builder.AppendJoin(' ', [
             $"-i \"{streamUrl}\"",
             "-v quiet",
             "-stats",
             "-y",
             "-c copy",
             $"\"{filePath}\""
            ]);

        Process processor = new()
        {
            StartInfo = new(ffmpegLocation)
            {
                Arguments = builder.ToString(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            }
        };
        if (onDataReceived is not null)
            processor.ErrorDataReceived += onDataReceived;

        return processor;
    }


    public async Task<string> GetVoeStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        string webContent = await requestHelper.GetAndValidateAsync(videoUrl, null, null, cancellationToken);

        HtmlNode root;

        // Extract video url from redirect
        Match redirectMatch = VoeVideoRedirectRegex().Match(webContent);
        if (redirectMatch.Success)
        {
            root = await GetHtmlRootAsync(redirectMatch.Groups[1].Value, cancellationToken);
        }
        else
        {
            HtmlDocument document = new();
            document.LoadHtml(webContent);

            root = document.DocumentNode;
        }    

        // Extract stream url from video
        logger?.LogInformation("[DownloadClient-GetVoeStreamUrlAsync] Extracting VOE stream url from video: {videoUrl}...", redirectMatch.Groups[1].Value);

        string streamUrlJs = root.SelectSingleNodeText("//script[contains(text(), 'var sources')]");
        Match streamUrlMatch = VoeStreamUrlRegex().Match(streamUrlJs);
        if (streamUrlMatch.Success)
        {
            byte[] encodedData = Convert.FromBase64String(streamUrlMatch.Groups[1].Value);
            return Encoding.UTF8.GetString(encodedData);
        }

        // Extract stream data from video
        logger?.LogInformation("[DownloadClient-GetVoeStreamUrlAsync] Extracting VOE stream url failed. Extracting VOE stream data from video now");

        string streamDataJs = root.SelectSingleNodeText("//script[contains(text(), 'let f62aad852c654bf8c9737da67c45630c7dec5019')]");
        Match streamDataMatch = VoeStreamDataRegex().Match(streamDataJs);
        if (streamDataMatch.Success)
        {
            byte[] encodedData = Convert.FromBase64String(streamDataMatch.Groups[1].Value);
            IEnumerable<char> json = Encoding.UTF8.GetString(encodedData).Reverse();

            string? streamUrl = JsonDocument.Parse(json.ToArray()).RootElement.GetProperty("file").GetString();
            if (streamUrl is not null)
                return streamUrl;
        }

        throw new UrlExtractionFailedException(videoUrl);
    }

    public async Task<string> GetStreamtapeStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        // Extract video url from redirect
        if (!videoUrl.Contains("/e/"))
        { 
            HtmlNode newRoot = await GetHtmlRootAsync(videoUrl, cancellationToken);
            videoUrl = newRoot.SelectSingleNodeAttribute("//meta[@name='og:url']", "content");
        }

        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync(videoUrl.Replace("/e/", "/v/"), cancellationToken);

        // Extract stream url from video
        logger?.LogInformation("[DownloadClient-GetStreamtapeStreamUrlAsync] Extracting voe stream url from video: {videoUrl}...", videoUrl);

        Match norobotLinkMatch = StreamtapeNoRobotRegex().Match(root.InnerHtml);
        if (!norobotLinkMatch.Success)
            throw new UrlExtractionFailedException(videoUrl);

        Match tokenMatch = StreamtapeTokenRegex().Match(norobotLinkMatch.Groups[1].Value);
        if (!tokenMatch.Success)
            throw new UrlExtractionFailedException(videoUrl);

        string hostUrl = root.SelectSingleNodeText("//div[@id='ideoooolink' and @style='display:none;']");
        return $"https://{hostUrl}&token={tokenMatch.Groups[1].Value}&dl=1s";
    }

    public async Task<string> GetDoodstreamStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync(videoUrl, cancellationToken);

        // Extract stream url from video
        logger?.LogInformation("[DownloadClient-GetDoodstreamStreamUrlAsync] Extracting voe stream url from video: {videoUrl}...", videoUrl);

        string js = root.SelectSingleNodeText("//script[contains(text(), '/pass_md5/')]");

        Match match = DoodstreamPassMd5Regex().Match(js);
        if (!match.Success)
            throw new UrlExtractionFailedException(videoUrl);

        string passMd5 = match.Groups[1].Value;
        string streamUrl = await requestHelper.GetAndValidateAsync($"https://dood.li/pass_md5/{passMd5}", null, [("Referer", videoUrl)], cancellationToken);
        long expiry = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return $"{streamUrl}{RandomString(10)}?token={passMd5}&expiry={expiry}";
    }

    public async Task<string> GetVidozaStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync(videoUrl, cancellationToken);

        // Extract stream url from video
        logger?.LogInformation("[DownloadClient-GetVoeStreamUrlAsync] Extracting voe stream url from video: {videoUrl}...", videoUrl);

        return root.SelectSingleNodeAttribute("//video[@id='player']/source", "src");
    }


    public Task DownloadAsync(
        string streamUrl,
        string filePath,
        (string key, string value)[]? headers = null,
        IProgress<EncodingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[DownloadClient-DownloadAsync] Starting to download & encode stream...");

        Process encoder = CreateEncoder(streamUrl, filePath, headers, progress is null ? null : new((s, e) =>
        {
            if (e.Data is null)
                return;

            Match match = FFmpegEncodingProgressRegex().Match(e.Data);
            if (!match.Success)
                return;

            int framesProcessed = match.Groups[1].Value.ToInt32();
            double fps = match.Groups[2].Value.ToDouble();
            double quality = match.Groups[3].Value.ToDouble();
            int outputFileSizeKb = match.Groups[4].Value.ToInt32();
            TimeSpan timeElapsed = match.Groups[5].Value.ToTimeSpan();
            double bitrateKbps = match.Groups[6].Value.ToDouble();
            double speedMultiplier = match.Groups[7].Value.ToDouble();

            progress.Report(new(
                framesProcessed: framesProcessed,
                fps: fps,
                quality: quality,
                outputFileSizeKb: outputFileSizeKb,
                timeElapsed: timeElapsed,
                bitrateKbps: bitrateKbps,
                speedMultiplier: speedMultiplier));
        }));

        cancellationToken.Register(async () =>
        {
            try
            {
                encoder.Kill();
                await Task.Delay(1000, CancellationToken.None);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[DownloadClient-OnDownloadCancellationTokenCanceled] Failed to kill encoder and delete file: {exception}", ex.Message);
            }
        });

        encoder.Start();
        encoder.BeginErrorReadLine();

        return encoder.WaitForExitAsync(cancellationToken);
    }
}