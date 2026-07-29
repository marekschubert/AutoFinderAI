using AutoFinderAI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Infrastructure.Crawling;

/// <summary>
/// Politeness-aware HTML fetcher: descriptive User-Agent, request timeout, and relies on the
/// named "Crawler" HttpClient's standard resilience handler (registered in
/// Infrastructure.DependencyInjection) for retry-with-backoff on 429/5xx.
/// </summary>
public sealed class HttpHtmlFetcher : IHtmlFetcher
{
    private readonly HttpClient _httpClient;
    private readonly CrawlerOptions _options;
    private readonly ILogger<HttpHtmlFetcher> _logger;

    public HttpHtmlFetcher(IHttpClientFactory httpClientFactory, IOptions<CrawlerOptions> options, ILogger<HttpHtmlFetcher> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Crawler");
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> FetchAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Url} returned {StatusCode}.", url, (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Failed to fetch {Url}.", url);
            return null;
        }
    }
}
