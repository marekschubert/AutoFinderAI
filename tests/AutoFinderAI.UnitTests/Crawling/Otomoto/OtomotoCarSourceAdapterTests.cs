using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;
using AutoFinderAI.Infrastructure.Crawling;
using AutoFinderAI.Infrastructure.Crawling.Otomoto;
using AutoFinderAI.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoFinderAI.UnitTests.Crawling.Otomoto;

public class OtomotoCarSourceAdapterTests
{
    private const string FirstPageUrl = "https://www.otomoto.pl/osobowe?search%5Border%5D=created_at_first%3Adesc";
    private const string SecondPageUrl = "https://www.otomoto.pl/osobowe?page=2&search%5Border%5D=created_at_first%3Adesc";

    [Fact]
    public async Task CrawlAsync_uses_detail_page_publication_date_to_accept_recent_listings()
    {
        const string detailUrl = "https://www.otomoto.pl/osobowe/oferta/bmw-seria-3-ID6Ib2xn.html";
        var listHtml = CreateListHtml("6149575845", detailUrl, "BMW Seria 3");
        var detailHtml = LoadFixture("single_car_offer_critical_elements.html");
        var fetcher = new RecordingHtmlFetcher(new Dictionary<string, string>
        {
            [FirstPageUrl] = listHtml,
            [detailUrl] = detailHtml
        });
        var now = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var adapter = CreateAdapter(fetcher, now, maxPages: 1);

        var listings = await CrawlAllAsync(adapter);

        listings.Should().ContainSingle();
        listings[0].ExternalId.Should().Be("6149575845");
        listings[0].Title.Should().Be("BMW Seria 3");
        listings[0].PublishedAt.Should().Be(new DateTime(2026, 7, 29, 10, 7, 0, DateTimeKind.Utc));
        listings[0].MakeText.Should().Be("BMW");
    }

    [Fact]
    public async Task CrawlAsync_stops_pagination_when_list_page_has_no_recent_detail_pages()
    {
        const string oldDetailUrl = "https://www.otomoto.pl/osobowe/oferta/old.html";
        const string recentDetailUrl = "https://www.otomoto.pl/osobowe/oferta/recent.html";
        var fetcher = new RecordingHtmlFetcher(new Dictionary<string, string>
        {
            [FirstPageUrl] = CreateListHtml("old", oldDetailUrl, "Old listing"),
            [oldDetailUrl] = CreateDetailHtml("28 lipca 2026 10:00"),
            [SecondPageUrl] = CreateListHtml("recent", recentDetailUrl, "Recent listing"),
            [recentDetailUrl] = CreateDetailHtml("29 lipca 2026 12:07")
        });
        var now = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var adapter = CreateAdapter(fetcher, now, maxPages: 2);

        var listings = await CrawlAllAsync(adapter);

        listings.Should().BeEmpty();
        fetcher.FetchCount(SecondPageUrl).Should().Be(0);
        fetcher.FetchCount(recentDetailUrl).Should().Be(0);
    }

    private static OtomotoCarSourceAdapter CreateAdapter(IHtmlFetcher fetcher, DateTime now, int maxPages)
    {
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(now);

        return new OtomotoCarSourceAdapter(
            fetcher,
            dateTimeProvider,
            Options.Create(new CrawlerOptions
            {
                MaxPages = maxPages,
                RequestDelayMs = 0,
                MaxDetailFetches = 20
            }),
            Substitute.For<ILogger<OtomotoCarSourceAdapter>>(),
            Substitute.For<ILogger<OtomotoListPageParser>>());
    }

    private static async Task<List<RawListing>> CrawlAllAsync(OtomotoCarSourceAdapter adapter)
    {
        var results = new List<RawListing>();
        await foreach (var listing in adapter.CrawlAsync(VehicleCategory.Car, CancellationToken.None))
        {
            results.Add(listing);
        }

        return results;
    }

    private static string LoadFixture(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "Fixtures", "otomoto")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir is null
            ? throw new DirectoryNotFoundException("Fixtures/otomoto directory not found.")
            : File.ReadAllText(Path.Combine(dir, "Fixtures", "otomoto", fileName));
    }

    private static string CreateListHtml(string id, string url, string title) => $"""
        <div data-testid="search-results">
            <article data-id="{id}">
                <h2><a href="{url}">{title}</a></h2>
                <ul>
                    <li><p>Warszawa (Mazowieckie)</p></li>
                    <li><p>Prywatny sprzedawca • Opublikowano</p></li>
                </ul>
            </article>
        </div>
        """;

    private static string CreateDetailHtml(string publishedAt) => $"""
        <div data-testid="content-description-section">
            <div><p>{publishedAt}</p></div>
        </div>
        <div data-testid="make"><div><p>Marka pojazdu</p><p>BMW</p></div></div>
        """;

    private sealed class RecordingHtmlFetcher : IHtmlFetcher
    {
        private readonly IReadOnlyDictionary<string, string> _htmlByUrl;
        private readonly Dictionary<string, int> _fetchCounts = new(StringComparer.Ordinal);

        public RecordingHtmlFetcher(IReadOnlyDictionary<string, string> htmlByUrl)
        {
            _htmlByUrl = htmlByUrl;
        }

        public Task<string?> FetchAsync(string url, CancellationToken cancellationToken)
        {
            _fetchCounts[url] = FetchCount(url) + 1;
            return Task.FromResult(_htmlByUrl.TryGetValue(url, out var html) ? html : null);
        }

        public int FetchCount(string url) => _fetchCounts.TryGetValue(url, out var count) ? count : 0;
    }
}
