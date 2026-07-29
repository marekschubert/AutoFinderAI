using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Ai.CriteriaExtraction;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Ai;

public class CriteriaSanitizerTests
{
    private sealed class FakeAiSearchOptions : IAiSearchOptions
    {
        public int MaxCandidates => 200;
        public int DefaultLimit => 10;
        public int MaxLimit => 50;
        public int MaxRepairRetries => 1;
    }

    private static readonly IAiSearchOptions Options = new FakeAiSearchOptions();

    [Fact]
    public void Sanitize_HappyPath_ProducesExpectedCriteria()
    {
        var raw = new RawCriteriaDto { Make = "BMW", Model = "3 Series", YearFrom = 2018, PriceTo = 100000m };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria.Should().NotBeNull();
        criteria!.Make.Should().Be("BMW");
        criteria.Model.Should().Be("3 Series");
        criteria.MinYear.Should().Be(2018);
        criteria.MaxPrice.Should().Be(100000m);
        criteria.Limit.Should().Be(10);
    }

    [Fact]
    public void Sanitize_ReversedYearRange_SwapsValues()
    {
        var raw = new RawCriteriaDto { YearFrom = 2020, YearTo = 2010 };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.MinYear.Should().Be(2010);
        criteria.MaxYear.Should().Be(2020);
    }

    [Fact]
    public void Sanitize_ReversedPriceRange_SwapsValues()
    {
        var raw = new RawCriteriaDto { PriceFrom = 100000m, PriceTo = 20000m };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.MinPrice.Should().Be(20000m);
        criteria.MaxPrice.Should().Be(100000m);
    }

    [Fact]
    public void Sanitize_OutOfRangeValues_AreClamped()
    {
        var raw = new RawCriteriaDto { YearFrom = 1800, MileageMax = 50_000_000, EnginePowerHpFrom = 99999, SeatsMin = 100 };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.MinYear.Should().Be(1950);
        criteria.MaxMileage.Should().Be(2_000_000);
        criteria.MinPowerHp.Should().Be(2000);
        criteria.SeatsMin.Should().Be(9);
    }

    [Fact]
    public void Sanitize_UnknownEnumValue_IsDropped()
    {
        var raw = new RawCriteriaDto { Make = "BMW", FuelType = "Nuclear", Transmission = "Warp", BodyType = "Truck" };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.FuelType.Should().BeNull();
        criteria.Transmission.Should().BeNull();
        criteria.BodyType.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 50)]
    [InlineData(5, 5)]
    public void Sanitize_Limit_IsClampedBetween1AndMax(int requested, int expected)
    {
        var raw = new RawCriteriaDto { Make = "BMW", Limit = requested };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.Limit.Should().Be(expected);
    }

    [Fact]
    public void Sanitize_NoLimitRequested_UsesDefaultLimit()
    {
        var raw = new RawCriteriaDto { Make = "BMW" };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.Limit.Should().Be(10);
    }

    [Fact]
    public void Sanitize_NoFilters_ReturnsNullCriteria()
    {
        var raw = new RawCriteriaDto { ClarificationQuestion = "What car do you want?" };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria.Should().BeNull();
    }

    [Fact]
    public void Sanitize_DuplicateKeywords_AreDeduplicated()
    {
        var raw = new RawCriteriaDto { Make = "BMW", Keywords = new List<string> { "sport", "Sport", " sport ", "premium" } };

        var (criteria, _) = CriteriaSanitizer.Sanitize(raw, Options);

        criteria!.Keywords.Should().HaveCount(2);
    }
}
