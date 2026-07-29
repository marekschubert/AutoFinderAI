using AngleSharp.Html.Parser;
using AutoFinderAI.Infrastructure.Crawling.Otomoto;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Crawling.Otomoto;

public class OtomotoDetailPageParserTests
{
    private static readonly HtmlParser HtmlParser = new();

    private static string LoadFixture(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "Fixtures", "otomoto")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        if (dir is null)
        {
            throw new DirectoryNotFoundException("Fixtures/otomoto directory not found.");
        }

        return File.ReadAllText(Path.Combine(dir, "Fixtures", "otomoto", fileName));
    }

    [Fact]
    public void Parse_extracts_all_fields_from_the_real_detail_page_fixture()
    {
        var html = LoadFixture("single_car_offer_critical_elements.html");
        var document = HtmlParser.ParseDocument(html);

        var details = new OtomotoDetailPageParser().Parse(document);

        details.PublishedAt.Should().Be(new DateTime(2026, 7, 29, 10, 7, 0, DateTimeKind.Utc));
        details.Make.Should().Be("BMW");
        details.Model.Should().Be("Seria 3");
        details.Year.Should().Be("2007");
        details.Mileage.Should().Be("56 377 km");
        details.FuelType.Should().Be("Diesel");
        details.Gearbox.Should().Be("Automatyczna");
        details.EnginePower.Should().Be("163 KM");
        details.EngineCapacity.Should().Be("1 995 cm3");
        details.BodyType.Should().Be("Sedan");
        details.Drive.Should().Be("Na tylne koła");
        details.Doors.Should().Be("4");
        details.Seats.Should().Be("5");
        details.Color.Should().Be("Czarny");
        details.CountryOfOrigin.Should().Be("Niemcy");
        details.NoAccident.Should().Be("Tak");
        details.OriginalOwner.Should().Be("Tak");
        details.PriceAmount.Should().Be("38 900");
        details.Currency.Should().Be("PLN");
    }

    [Fact]
    public void Parse_falls_back_to_detail_label_cards_when_specific_testid_is_missing()
    {
        // Only the fallback [data-testid="detail"] quick-facts cards are present (no fuel_type
        // testid element), mirroring the shape found in car_offers_critical_elements.html-style
        // pages where some attributes only appear in the quick-facts summary.
        const string html = """
            <div data-testid="detail"><div><p>Rodzaj paliwa</p><p>Diesel</p></div></div>
            <div data-testid="detail"><div><p>Przebieg</p><p>10 000 km</p></div></div>
            """;

        var document = HtmlParser.ParseDocument(html);
        var details = new OtomotoDetailPageParser().Parse(document);

        details.FuelType.Should().Be("Diesel");
        details.Mileage.Should().Be("10 000 km");
        details.PublishedAt.Should().BeNull();
        details.Make.Should().BeNull();
    }

    [Fact]
    public void Parse_never_throws_when_all_fields_are_missing()
    {
        var document = HtmlParser.ParseDocument("<div>empty page</div>");

        var act = () => new OtomotoDetailPageParser().Parse(document);

        act.Should().NotThrow();
        act().Make.Should().BeNull();
    }

    [Theory]
    [InlineData("29 lipca 2026 12:07", 2026, 7, 29, 12, 7)]
    [InlineData("1 lipca 2026 2:07", 2026, 7, 1, 2, 7)]
    [InlineData("31 grudnia 2025 23:59", 2025, 12, 31, 23, 59)]
    [InlineData("1 stycznia 2026 0:00", 2026, 1, 1, 0, 0)]
    public void PublishedAtParser_parses_polish_publication_dates(
        string text,
        int year,
        int month,
        int day,
        int hour,
        int minute)
    {
        var parsed = OtomotoPublishedAtParser.TryParse(text, TimeZoneInfo.Utc, out var publishedAt);

        parsed.Should().BeTrue();
        publishedAt.Should().Be(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void PublishedAtParser_rejects_vehicle_registration_date_without_time()
    {
        var parsed = OtomotoPublishedAtParser.TryParse("9 marca 2007", TimeZoneInfo.Utc, out _);

        parsed.Should().BeFalse();
    }
}
