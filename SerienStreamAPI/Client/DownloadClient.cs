using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SerienStreamAPI.Internal;
using System.Text.RegularExpressions;
using System.Text;
using SerienStreamAPI.Exceptions;

namespace SerienStreamAPI.Client;

public partial class DownloadClient
{
    [GeneratedRegex(@"'hls':\s*'(.*?)'")]
    private static partial Regex VoeStreamUrlRegex();

    [GeneratedRegex("document\\.getElementById\\('norobotlink'\\)\\.innerHTML = (.+);")]
    private static partial Regex StreamtapeNoRobotRegex();

    [GeneratedRegex("token=([^&']+)")]
    private static partial Regex StreamtapeTokenRegex();
    
    [GeneratedRegex(@"/pass_md5/([^/]+/[^']+)")]
    private static partial Regex DoodstreamPassMd5Regex();


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


    readonly ILogger<DownloadClient>? logger;

    readonly RequestHelper requestHelper;

    public DownloadClient(
        ILogger<DownloadClient>? logger = null)
    {
        this.logger = logger;

        this.requestHelper = new(false, logger);

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


    public async Task<string> GetVoeStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync(videoUrl, cancellationToken);

        // Extract stream url from video
        logger?.LogInformation("[DownloadClient-GetVoeStreamUrlAsync] Extracting voe stream url from video: {videoUrl}...", videoUrl);

        string js = root.SelectSingleNodeText("//script[contains(text(), 'var sources')]");

        Match match = VoeStreamUrlRegex().Match(js);
        if (!match.Success)
            throw new UrlExtractionFailedException(videoUrl);

        byte[] encodedData = Convert.FromBase64String(match.Groups[1].Value);
        return Encoding.UTF8.GetString(encodedData);
    }

    public async Task<string> GetStreamtapeStreamUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
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

}