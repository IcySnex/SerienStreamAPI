using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SerienStreamAPI.Enums;
using SerienStreamAPI.Exceptions;
using SerienStreamAPI.Internal;
using SerienStreamAPI.Models;

namespace SerienStreamAPI.Client;

public class SerienStreamClient
{
    readonly string hostUrl;
    readonly string site;
    readonly ILogger<SerienStreamClient>? logger;

    readonly RequestHelper requestHelper;

    public SerienStreamClient(
        string hostUrl,
        string site,
        bool ignoreCertificateValidation = false,
        ILogger<SerienStreamClient>? logger = null)
    {
        this.hostUrl = hostUrl;
        this.site = site;
        this.logger = logger;

        this.requestHelper = new(ignoreCertificateValidation, logger);

        logger?.LogInformation("[SerienStreamClient-.ctor] SerienStreamClient has been initialized.");
    }


    async Task<HtmlNode> GetHtmlRootAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("[SerienStreamClient-GetHtmlRootAsync] Getting HTML document: {path}...", path);
        string webContent = await requestHelper.GetAndValidateAsync(hostUrl, path, null, cancellationToken);

        HtmlDocument document = new();
        document.LoadHtml(webContent);

        return document.DocumentNode;
    }


    public async Task<Series> GetSeriesAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        // Get HTML document
        HtmlNode root = await GetHtmlRootAsync($"{site}/{title.ToRelativePath()}", cancellationToken);

        if (root.Any("//div[contains(@class, 'messageAlert danger')]"))
            throw new SeriesNotFoundException(title);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetSeriesAsync] Parsing HTML document into series info: {title}...", title);

        string endYearText = root.SelectSingleNodeText("//p[contains(@class,'text-muted')]/span[1]");

        return new Series(
            title: root.SelectSingleNodeText("//div[contains(@class,'row')]//h1"),
            description: root.SelectSingleNodeText("//div[contains(@class,'series-description')]//span[@class='description-text']"),
            bannerUrl: hostUrl.AddRelativePath(root.SelectSingleNodeAttribute("//div[contains(@class,'col-12') and contains(@class,'col-md-9')]//picture//img", "data-src")),
            yearStart: root.SelectSingleNodeText("//p[contains(@class,'text-muted')]/a[1]").ToInt32(),
            yearEnd: endYearText == "NA" ? null : endYearText.ToInt32(),
            directors: root.Select("//li[strong[contains(text(),'Regisseur')]]//a", Extensions.GetInnerText),
            actors: root.Select("//li[strong[contains(text(),'Besetzung')]]//a", Extensions.GetInnerText),
            creators: root.Select("//li[strong[contains(text(),'Produzent')]]//a", Extensions.GetInnerText),
            countriesOfOrigin: root.Select("//li[strong[contains(text(),'Land')]]//a", Extensions.GetInnerText),
            genres: root.Select("//li[strong[contains(text(),'Genre')]]//a", Extensions.GetInnerText),
            ageRating: root.SelectSingleNodeText("//p[contains(.,'FSK')]").Match(@"FSK (\d+)", 1).ToInt32(),
            ratingsCount: root.SelectSingleNodeText("//span[contains(text(),'Bewertungen')]").Match(@"([\d|\.|\,]+) Bewertungen", 1).ToInt32(),
            imdbUrl: root.SelectSingleNodeAttributeOrDefault("//a[contains(@href,'imdb.com')]", "href"),
            trailerUrl: root.SelectSingleNodeAttributeOrDefault("//button[@data-trailer-url]", "data-trailer-url"),
            hasMovies: root.Any("//nav[@id='season-nav']//ul/li/a[normalize-space(text())='Filme']"),
            seasonsCount: root.Select("//nav[@id='season-nav']//ul/li/a[normalize-space(text()) != 'Filme']", n => n).Length);
    }


    public async Task<Media[]> GetEpisodesAsync(
        string title,
        int season,
        CancellationToken cancellationToken = default)
    {
        // Get HTML document
        HtmlNode root = await GetHtmlRootAsync($"{site}/{title.ToRelativePath()}/staffel-{season}", cancellationToken);

        if (root.ChildNodes.Count == 0)
            return season == 0 ? [] : throw new SeasonNotFoundException(title, season);
        if (root.Any("//div[contains(@class, 'messageAlert danger')]"))
            throw new SeriesNotFoundException(title);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetEpisodesAsync] Parsing HTML document into media info list: {title}, {season}...", title, season);

        return root.Select("//section[contains(@class,'episode-section')]//tbody//tr[contains(@class,'episode-row')]", node => new Media(
            number: node.SelectSingleNodeText(".//th[contains(@class,'episode-number-cell')]").ToInt32(),
            title: node.SelectSingleNodeText(".//td[contains(@class,'episode-title-cell')]//strong"),
            originalTitle: node.SelectSingleNodeText(".//td[contains(@class,'episode-title-cell')]//span"),
            hosters: node.Select(".//td[contains(@class,'episode-watch-cell')]//img", childNode => childNode.GetAttributeValue("alt").ToHoster()),
            languages: node.Select(".//td[contains(@class,'episode-language-cell')]//svg//use", childNode => childNode.GetAttributeValue("href").ToMediaLanguage())));
    }

    public Task<Media[]> GetMoviesAsync(
        string title,
        CancellationToken cancellationToken = default) =>
        GetEpisodesAsync(title, 0, cancellationToken);


    public async Task<VideoDetails> GetEpisodeVideoInfoAsync(
        string title,
        int number,
        int season,
        CancellationToken cancellationToken = default)
    {
        // Get HTML document
        HtmlNode root = await GetHtmlRootAsync($"{site}/{title.ToRelativePath()}/staffel-{season}/episode-{number}", cancellationToken);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetEpisodeVideoInfoAsync] Parsing HTML document into video info: {title}, {number}, {season}...", title, number, season);

        string fullTitle = root.SelectSingleNodeText("//article/h2[@class='h4 mb-1']")["S00E00:".Length..].Trim();
        string currentInfo = root.SelectSingleNodeText("//div[@class='small mx-2']/span/strong");
        
        return new VideoDetails(
            number: currentInfo.Match(@"E(\d+)", 1).ToInt32(),
            season: currentInfo.Contains("S00") ? null : currentInfo.Match(@"E(\d+)", 1).ToInt32(),
            title: fullTitle.Match(@"(.*?)(?:\s*\(([^()]*)\))?\s*$", 1),
            originalTitle: fullTitle.Match(@"(.*?)(?:\s*\(([^()]*)\))?\s*$", 2),
            description: root.SelectSingleNodeText("//div[starts-with(@id,'desc-')]/div"),
            streams: root.Select("//div[@id='episode-links']//button[contains(@class,'link-box')]", node => new VideoStream(
                videoUrl: hostUrl.AddRelativePath(node.GetAttributeValue("data-play-url")),
                hoster: node.GetAttributeValue("data-provider-name").ToHoster(),
                language: node.SelectSingleNodeAttribute(".//use", "href").ToMediaLanguage())));
    }

    public Task<VideoDetails> GetMovieVideoInfoAsync(
        string title,
        int number,
        CancellationToken cancellationToken = default) =>
        GetEpisodeVideoInfoAsync(title, number, 0, cancellationToken);
}
