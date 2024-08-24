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
        bool ignoreCerficiateValidation = false,
        ILogger<SerienStreamClient>? logger = null)
    {
        this.hostUrl = hostUrl;
        this.site = site;
        this.logger = logger;

        this.requestHelper = new(ignoreCerficiateValidation, logger);

        logger?.LogInformation("[SerienStreamClient-.ctor] SerienStreamClient has been inizialized.");
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
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync($"{site}/stream/{title.ToRelativePath()}", cancellationToken);

        if (root.Any("//div[contains(@class, 'messageAlert danger')]"))
            throw new SeriesNotFoundException(title);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetSeriesAsync] Parsing HTML document into series info: {title}...", title);

        string endYearText = root.SelectSingleNodeText("//span[@itemprop='endDate']/a");
        bool hasMovies = root.SelectSingleNodeTextOrDefault("//div[@id='stream']//ul[1]/li[2]/a") == "Filme";

        return new Series(
            title: root.SelectSingleNodeText("//div[@class='series-title']/h1"),
            description: root.SelectSingleNodeAttribute("//p[@class='seri_des']", "data-full-description"),
            bannerUrl: hostUrl.AddRelativePath(root.SelectSingleNodeAttribute("//div[@class='seriesCoverBox']//img", "data-src")),
            yearStart: root.SelectSingleNodeText("//span[@itemprop='startDate']/a").ToInt32(),
            yearEnd: endYearText == "Heute" ? null : endYearText.ToInt32(),
            directors: root.Select("//li[@itemprop='director']//span[@itemprop='name']", Extensions.GetInnerText),
            actors: root.Select("//li[@itemprop='actor']//span[@itemprop='name']", Extensions.GetInnerText),
            creators: root.Select("//li[@itemprop='creator']//span[@itemprop='name']", Extensions.GetInnerText),
            countriesOfOrigin: root.Select("//li[@itemprop='countryOfOrigin']//span[@itemprop='name']", Extensions.GetInnerText),
            genres: root.Select("//div[@class='genres']//li/a[@itemprop='genre']", Extensions.GetInnerText),
            ageRating: root.SelectSingleNodeAttribute("//div[contains(@class, 'fsk')]", "data-fsk").ToInt32(),
            rating: new Rating(
                root.SelectSingleNodeText("//div[@itemprop='aggregateRating']//span[@itemprop='ratingValue']").ToInt32(),
                root.SelectSingleNodeText("//div[@itemprop='aggregateRating']//span[@itemprop='bestRating']").ToInt32(),
                root.SelectSingleNodeText("//div[@itemprop='aggregateRating']//span[@itemprop='ratingCount']").ToInt32()),
            imdbUrl: root.SelectSingleNodeAttributeOrDefault("//a[@class='imdb-link']", "href"),
            trailerUrl: root.SelectSingleNodeAttributeOrDefault("//div[@itemprop='trailer']//a[@itemprop='url']", "href"),
            hasMovies: hasMovies,
            seasonsCount: root.SelectSingleNodeAttribute("//meta[@itemprop='numberOfSeasons']", "content").ToInt32() - hasMovies.ToInt32());
    }


    public async Task<Media[]> GetEpisodesAsync(
        string title,
        int season,
        CancellationToken cancellationToken = default)
    {
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync($"{site}/stream/{title.ToRelativePath()}/staffel-{season}", cancellationToken);

        if (root.ChildNodes.Count == 0)
            return season == 0 ? [] : throw new SeasonNotFoundException(title, season);
        if (root.Any("//div[contains(@class, 'messageAlert danger')]"))
            throw new SeriesNotFoundException(title);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetEpisodesAsync] Parsing HTML document into media info list: {title}, {season}...", title, season);

        return root.Select("//table[@class='seasonEpisodesList']//tbody//tr", node => new Media(
            number: node.SelectSingleNodeText(".//td[1]//a")[6..].ToInt32(),
            title: node.SelectSingleNodeText(".//td[2]/a/strong"),
            originalTitle: node.SelectSingleNodeText(".//td[2]/a/span"),
            hosters: node.Select(".//td[3]//i", childNode => childNode.GetAttributeValue("title").ToHoster()),
            languages: node.Select(".//td[4]//img", childNode => childNode.GetAttributeValue("src").ToMediaLanguage())));
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
        // Get HTML doucment
        HtmlNode root = await GetHtmlRootAsync($"{site}/stream/{title.ToRelativePath()}/staffel-{season}/episode-{number}", cancellationToken);

        if (root.ChildNodes.Count == 0)
            throw season == 0 ? new MovieNotFoundException(title, number) : new EpisodeNotFoundException(title, season, number);
        if (root.Any("//div[contains(@class, 'messageAlert danger')]"))
            throw new SeriesNotFoundException(title);
        if (!root.Any("//div[contains(@class, 'hosterSiteDirectNav')]"))
            throw new SeasonNotFoundException(title, season);

        // Parse HTML document into series info
        logger?.LogInformation("[SerienStreamClient-GetEpisodeVideoInfoAsync] Parsing HTML document into video info: {title}, {number}, {season}...", title, number, season);

        Dictionary<int, MediaLanguage> languageMapping = root.Map("//div[@class='changeLanguageBox']//img", node => (
            node.GetAttributeValue("data-lang-key").ToInt32(),
            node.GetAttributeValue("src").ToMediaLanguage()));

        return new VideoDetails(
            number: root.SelectSingleNodeText("//ul/li/a[@class='active' and @data-episode-id]").ToInt32(),
            title: root.SelectSingleNodeText("//div[@class='hosterSiteTitle']/h2/span[@class='episodeGermanTitle']"),
            originalTitle: root.SelectSingleNodeText("//div[@class='hosterSiteTitle']/h2/small[@class='episodeEnglishTitle']"),
            description: root.SelectSingleNodeText("//div[@class='hosterSiteTitle']/p[@itemprop='description']"),
            streams: root.Select("//ul[@class='row']/li", node => new VideoStream(
                videoUrl: hostUrl.AddRelativePath(node.SelectSingleNodeAttribute(".//a[@class='watchEpisode']", "href")),
                hoster: node.SelectSingleNodeText(".//h4").ToHoster(),
                language: languageMapping.GetValueOrDefault(node.GetAttributeValue("data-lang-key").ToInt32(), new(Language.Unknown, null)))));
    }

    public Task<VideoDetails> GetMovieVideoInfoAsync(
        string title,
        int number,
        CancellationToken cancellationToken = default) =>
        GetEpisodeVideoInfoAsync(title, number, 0, cancellationToken);
}