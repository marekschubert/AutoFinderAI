using AngleSharp.Html.Parser;
using AutoFinderAI.Infrastructure.Crawling.Otomoto;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Crawling.Otomoto;

public class OtomotoListPageParserTests
{
    private const string BaseUrl = "https://www.otomoto.pl";
    private static readonly HtmlParser HtmlParser = new();

    private static string LoadFixture(string fileName)
    {
        var path = Path.Combine(FindFixturesDirectory(), fileName);
        return File.ReadAllText(path);
    }

    private static string FindFixturesDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "Fixtures", "otomoto")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir is null
            ? throw new DirectoryNotFoundException("Fixtures/otomoto directory not found.")
            : Path.Combine(dir, "Fixtures", "otomoto");
    }

    private static OtomotoListPageParser CreateParser() => new();

    [Fact]
    public void Parse_against_real_fixture_with_link_markup_extracts_listing_links()
    {
        var html = LoadFixture("car_offers_critical_elements.html");
        var document = HtmlParser.ParseDocument(html);

        var parser = CreateParser();

        var items = parser.Parse(document, BaseUrl);

        items.Should().HaveCount(2);
        items[0].ExternalId.Should().Be("6149575846");
        items[0].Url.Should().Be("https://www.otomoto.pl/osobowe/oferta/opel-crossland-x-ID6Ib2yL.html");
        items[0].Title.Should().Be("Opel Crossland X 1.2 Start/Stop Edition");
        items[1].ExternalId.Should().Be("6149575845");
        items[1].Url.Should().Be("https://www.otomoto.pl/osobowe/oferta/bmw-seria-3-ID6Ib2xn.html");
        items[1].Title.Should().Be("BMW Seria 3 320d xDrive M Sport Shadow");
    }

    [Fact]
    public void Parse_extracts_listing_link_when_link_markup_is_present()
    {
        // Reuses the exact "Opublikowano 6 godzin temu" publish-text found in
        // car_offers_critical_elements.html, with the h2>a link markup described in the otomoto
        // selector spec (stripped from that "critical elements" fixture for brevity) added back.
        const string html = """
            <div data-testid="search-results">
                <article data-id="6149575846">
                    <section>
                        <div>
                            <div><h2><a data-nextlink href="/oferta/bmw-seria-3-ID6Fp1a2.html">BMW Seria 3 2.0d</a></h2></div>
                            <ul>
                                <li><p>Gniezno (Wielkopolskie)</p></li>
                                <li><p>Prywatny sprzedawca &bull; Opublikowano 6 godzin temu</p></li>
                            </ul>
                        </div>
                    </section>
                </article>
            </div>
            """;

        var document = HtmlParser.ParseDocument(html);
        var parser = CreateParser();

        var items = parser.Parse(document, BaseUrl);

        items.Should().ContainSingle();
        var item = items[0];
        item.ExternalId.Should().Be("6149575846");
        item.Url.Should().Be("https://www.otomoto.pl/oferta/bmw-seria-3-ID6Fp1a2.html");
        item.Title.Should().Be("BMW Seria 3 2.0d");
    }

    [Fact]
    public void Parse_does_not_filter_by_list_page_publication_text()
    {
        const string html = """
            <div data-testid="search-results">
                <article data-id="1">
                    <section>
                        <div>
                            <div><h2><a href="/oferta/x.html">X</a></h2></div>
                            <ul>
                                <li><p>Podbite</p></li>
                            </ul>
                        </div>
                    </section>
                </article>
            </div>
            """;

        var document = HtmlParser.ParseDocument(html);
        var parser = CreateParser();

        parser.Parse(document, BaseUrl).Should().ContainSingle(item => item.ExternalId == "1");
    }

    [Fact]
    public void Parse_returns_empty_when_search_results_container_is_missing()
    {
        var document = HtmlParser.ParseDocument("<div>no results here</div>");
        var parser = CreateParser();

        parser.Parse(document, BaseUrl).Should().BeEmpty();
    }
}
