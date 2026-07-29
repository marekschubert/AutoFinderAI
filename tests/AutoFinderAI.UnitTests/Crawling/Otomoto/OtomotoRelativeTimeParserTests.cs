using AutoFinderAI.Infrastructure.Crawling.Otomoto;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Crawling.Otomoto;

public class OtomotoRelativeTimeParserTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("Opublikowano 5 sekund temu")]
    [InlineData("Opublikowano 1 sekundę temu")]
    [InlineData("Opublikowano 30 minut temu")]
    [InlineData("Opublikowano 1 minutę temu")]
    [InlineData("Opublikowano 6 godzin temu")]
    [InlineData("Opublikowano 1 godzinę temu")]
    [InlineData("Prywatny sprzedawca • Opublikowano 6 godzin temu")]
    public void TryParseRecent_returns_true_for_second_minute_hour_phrasing(string text)
    {
        var result = OtomotoRelativeTimeParser.TryParseRecent(text, Now, out var publishedAt);

        result.Should().BeTrue();
        publishedAt.Should().BeOnOrBefore(Now).And.BeOnOrAfter(Now.AddDays(-1));
    }

    [Theory]
    [InlineData("Opublikowano wczoraj")]
    [InlineData("Opublikowano 2 dni temu")]
    [InlineData("Opublikowano tydzień temu")]
    [InlineData("Opublikowano 3 tygodnie temu")]
    [InlineData("Podbite")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParseRecent_returns_false_for_old_or_non_publish_phrasing(string? text)
    {
        var result = OtomotoRelativeTimeParser.TryParseRecent(text, Now, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseRecent_computes_expected_absolute_timestamp()
    {
        OtomotoRelativeTimeParser.TryParseRecent("Opublikowano 6 godzin temu", Now, out var publishedAt);

        publishedAt.Should().Be(Now.AddHours(-6));
    }
}
