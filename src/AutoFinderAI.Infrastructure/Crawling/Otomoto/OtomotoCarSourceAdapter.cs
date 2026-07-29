using System.Runtime.CompilerServices;
using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;
using AutoFinderAI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>
/// otomoto.pl passenger-car ("osobowe") listing source. Walks the newest-first search results
/// page by page; the list page is only used to discover detail-page URLs. Recency is decided from
/// the full publication date on the detail page, because otomoto loads the list-page relative
/// publication text asynchronously.
/// </summary>
public sealed class OtomotoCarSourceAdapter : IListingSourceAdapter
{
    private const string BaseUrl = "https://www.otomoto.pl";
    private const string FirstPageUrl = "https://www.otomoto.pl/osobowe?search%5Border%5D=created_at_first%3Adesc";
    private static readonly JsonSerializerOptions DebugJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public string SourceKey => "otomoto.pl";

    public IReadOnlyCollection<VehicleCategory> Supported { get; } = new[] { VehicleCategory.Car };

    private readonly IHtmlFetcher _htmlFetcher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly OtomotoListPageParser _listPageParser;
    private readonly OtomotoDetailPageParser _detailPageParser;
    private readonly CrawlerOptions _options;
    private readonly ILogger<OtomotoCarSourceAdapter> _logger;
    private readonly HtmlParser _htmlParser = new();

    public OtomotoCarSourceAdapter(
        IHtmlFetcher htmlFetcher,
        IDateTimeProvider dateTimeProvider,
        IOptions<CrawlerOptions> options,
        ILogger<OtomotoCarSourceAdapter> logger,
        ILogger<OtomotoListPageParser> listPageParserLogger)
    {
        _htmlFetcher = htmlFetcher;
        _dateTimeProvider = dateTimeProvider;
        _listPageParser = new OtomotoListPageParser(listPageParserLogger);
        _detailPageParser = new OtomotoDetailPageParser();
        _options = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<RawListing> CrawlAsync(
        VehicleCategory category,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (category != VehicleCategory.Car)
        {
            yield break;
        }

        var detailFetchCount = 0;

        for (var page = 1; page <= _options.MaxPages; page++)
        {
            var listUrl = BuildListUrl(page);
            var listHtml = await _htmlFetcher.FetchAsync(listUrl, cancellationToken);

            if (listHtml is null)
            {
                _logger.LogWarning("otomoto: list page {Page} could not be fetched ({Url}); stopping crawl.", page, listUrl);
                yield break;
            }

            var listDocument = _htmlParser.ParseDocument(listHtml);
            _logger.LogInformation(
                "otomoto: parsed list document for page {Page} ({Url}): {ListDocumentHtml}",
                page,
                listUrl,
                listDocument.DocumentElement?.OuterHtml);
            var listItems = _listPageParser.Parse(listDocument, BaseUrl);
            _logger.LogInformation(
                "otomoto: parsed list page {Page} ({Url}) listing links ({Count}): {ListItems}",
                page,
                listUrl,
                listItems.Count,
                JsonSerializer.Serialize(listItems, DebugJsonOptions));

            if (listItems.Count == 0)
            {
                _logger.LogInformation("otomoto: page {Page} has no listing links; stopping.", page);
                yield break;
            }

            var pageHadRecentListings = false;

            foreach (var item in listItems)
            {
                if (detailFetchCount >= _options.MaxDetailFetches)
                {
                    _logger.LogWarning("otomoto: reached MaxDetailFetches ({Max}); stopping crawl.", _options.MaxDetailFetches);
                    yield break;
                }

                await DelayAsync(cancellationToken);

                var detailHtml = await _htmlFetcher.FetchAsync(item.Url, cancellationToken);
                detailFetchCount++;

                if (detailHtml is null)
                {
                    _logger.LogWarning("otomoto: detail page could not be fetched ({Url}); skipping listing.", item.Url);
                    continue;
                }

                var detailDocument = _htmlParser.ParseDocument(detailHtml);
                var publishedAt = _detailPageParser.ParsePublishedAt(detailDocument);
                if (publishedAt is null)
                {
                    _logger.LogInformation(
                        "otomoto: detail page has no parseable publication date ({Url}); skipping listing.",
                        item.Url);
                    continue;
                }

                if (!IsPublishedInLast24Hours(publishedAt.Value, _dateTimeProvider.UtcNow))
                {
                    _logger.LogInformation(
                        "otomoto: detail page publication date {PublishedAt} is older than 24h ({Url}); skipping listing.",
                        publishedAt.Value,
                        item.Url);
                    continue;
                }

                pageHadRecentListings = true;
                var details = _detailPageParser.Parse(detailDocument, publishedAt);

                yield return ToRawListing(item, details, publishedAt.Value);
            }

            if (!pageHadRecentListings)
            {
                _logger.LogInformation("otomoto: page {Page} has no detail pages published in the last 24h; stopping.", page);
                yield break;
            }

            if (page < _options.MaxPages)
            {
                await DelayAsync(cancellationToken);
            }
        }
    }

    private static string BuildListUrl(int page) => page <= 1
        ? FirstPageUrl
        : $"https://www.otomoto.pl/osobowe?page={page}&search%5Border%5D=created_at_first%3Adesc";

    private Task DelayAsync(CancellationToken cancellationToken)
        => _options.RequestDelayMs > 0 ? Task.Delay(_options.RequestDelayMs, cancellationToken) : Task.CompletedTask;

    private static bool IsPublishedInLast24Hours(DateTime publishedAt, DateTime now)
        => publishedAt >= now.AddHours(-24) && publishedAt <= now;

    private static RawListing ToRawListing(OtomotoListItem item, OtomotoCarDetails details, DateTime publishedAt) => new(
        ExternalId: item.ExternalId,
        Url: item.Url,
        Title: item.Title,
        PublishedAt: publishedAt,
        PriceText: details.PriceAmount,
        CurrencyText: details.Currency,
        MakeText: details.Make,
        ModelText: details.Model,
        YearText: details.Year,
        MileageText: details.Mileage,
        FuelTypeText: details.FuelType,
        TransmissionText: details.Gearbox,
        EnginePowerText: details.EnginePower,
        EngineCapacityText: details.EngineCapacity,
        BodyTypeText: details.BodyType,
        DriveTypeText: details.Drive,
        DoorsText: details.Doors,
        SeatsText: details.Seats,
        ColorText: details.Color,
        CountryOfOriginText: details.CountryOfOrigin,
        NoAccidentText: details.NoAccident,
        OriginalOwnerText: details.OriginalOwner,
        LocationText: null,
        ThumbnailUrl: item.ThumbnailUrl);
}
