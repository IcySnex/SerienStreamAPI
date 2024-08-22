using Microsoft.Extensions.Logging;

namespace SerienStreamAPI.Internal;

/// <summary>
/// Helper which handles all HTTP requests
/// </summary>
internal class RequestHelper
{
    readonly ILogger? logger;

    readonly HttpClient httpClient;

    /// <summary>
    /// Creates a new request helper with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    public RequestHelper(
        bool ignoreCerficiateValidation = false,
        ILogger? logger = null)
    {
        this.logger = logger;

        HttpClientHandler handler = new();
        if (ignoreCerficiateValidation)
            handler.ServerCertificateCustomValidationCallback += (s, cert, chain, errors) => true;
        httpClient = new(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");

        logger?.LogInformation($"[RequestHelper-.ctor] RequestHelper with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Sends a new GET request to the given uri with the parameters
    /// </summary>
    /// <param name="url">The uri the request should be made to</param>
    /// <param name="path">The relatvie path of the request url</param>
    /// <param name="parameters">The query parameters which should be added</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="System.InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Threading.Tasks.TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The HTTP response message</returns>
    public Task<HttpResponseMessage> GetAsync(
        string url,
        string? path = null,
        (string key, string value)[]? headers = null,
        CancellationToken cancellationToken = default)
    {
        UriBuilder builder = new(url);
        if (path is not null)
            builder.Path = path;

        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Get,
            RequestUri = builder.Uri
        };
        if (headers is not null)
            foreach ((string key, string value) in headers)
                request.Headers.Add(key, value);

        logger?.LogInformation("[RequestHelper-GetAsync] Sending HTTP reuqest. GET: {url}.", url);
        return httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends a new GET request to the given uri with the parameters and validates it
    /// </summary>
    /// <param name="uri">The uri the request should be made to</param>
    /// <param name="path">The relatvie path of the request url</param>
    /// <param name="parameters">The query parameters which should be added</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="System.InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Threading.Tasks.TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The validated HTTP response data</returns>
    public async Task<string> GetAndValidateAsync(
        string uri,
        string? path = null,
        (string key, string value)[]? headers = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await GetAsync(uri, path, headers, cancellationToken).ConfigureAwait(false);

        logger?.LogInformation($"[RequestHelper-GetAndValidateAsync] Parsing HTTP response.");
        string httpResponseData = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger?.LogError("[RequestHelper-GetAndValidateAsync] HTTP request failed. Statuscode: {statusCode}.", httpResponse.StatusCode);
            throw new HttpRequestException($"HTTP request failed. StatusCode: {httpResponse.StatusCode}.", new(httpResponseData));
        }

        return httpResponseData;
    }
}