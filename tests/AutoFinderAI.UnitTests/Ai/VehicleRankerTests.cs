using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Ai.Ranking;
using AutoFinderAI.Domain.Enums;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Ai;

public class VehicleRankerTests
{
    private readonly VehicleRanker _ranker = new();

    private static VehicleDto CreateVehicle(
        string make = "BMW", string model = "3 Series", decimal price = 40000m, int year = 2019,
        int? mileage = 60000, FuelType fuelType = FuelType.Diesel, TransmissionType transmission = TransmissionType.Automatic,
        BodyType bodyType = BodyType.Kombi, int? seats = 5, int? powerHp = 150, bool? isDamaged = false,
        string title = "BMW 3 Series 2019", string? location = "Warsaw")
        => new(
            Guid.NewGuid(), "https://example.com/1", title, price, "PLN", make, model, null, year, mileage,
            fuelType, transmission, powerHp, null, location, null, DateTime.UtcNow, bodyType,
            4, seats, null, null, isDamaged, null, null);

    [Fact]
    public void Rank_MatchesMakeAndBudget_ProducesPositiveScoreAndReasons()
    {
        var criteria = new VehicleSearchCriteria(Make: "BMW", MaxPrice: 50000m);
        var vehicle = CreateVehicle(make: "BMW", price: 40000m);

        var ranked = _ranker.Rank(new[] { vehicle }, criteria);

        ranked.Should().HaveCount(1);
        ranked[0].Score.Should().BeGreaterThan(0);
        ranked[0].MatchReasons.Should().Contain(r => r.Contains("Make matches"));
        ranked[0].MatchReasons.Should().Contain(r => r.Contains("budget", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Rank_SoftPreferenceFamily_BoostsEstateWithSeatsOverCoupe()
    {
        var criteria = new VehicleSearchCriteria(SoftPreferences: new[] { "family" });
        var familyCar = CreateVehicle(bodyType: BodyType.Kombi, seats: 5);
        var sportsCar = CreateVehicle(bodyType: BodyType.Coupe, seats: 2);

        var ranked = _ranker.Rank(new[] { sportsCar, familyCar }, criteria);

        ranked.First().Vehicle.Should().Be(familyCar);
        ranked.First().MatchReasons.Should().Contain(r => r.Contains("Family"));
    }

    [Fact]
    public void Rank_SortByPriceAsc_OrdersByPriceRegardlessOfScore()
    {
        var criteria = new VehicleSearchCriteria(SortBy: VehicleSortBy.PriceAsc);
        var expensive = CreateVehicle(make: "BMW", price: 90000m);
        var cheap = CreateVehicle(make: "Toyota", price: 20000m);

        var ranked = _ranker.Rank(new[] { expensive, cheap }, criteria);

        ranked.First().Vehicle.Should().Be(cheap);
    }

    [Fact]
    public void Rank_NoCriteriaMatch_ReturnsZeroScoreNoReasons()
    {
        var criteria = new VehicleSearchCriteria();
        var vehicle = CreateVehicle();

        var ranked = _ranker.Rank(new[] { vehicle }, criteria);

        ranked[0].Score.Should().Be(0);
        ranked[0].MatchReasons.Should().BeEmpty();
    }
}
