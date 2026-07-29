using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Features.Crawling.Mapping;
using AutoFinderAI.Domain.Enums;
using FluentAssertions;
using DriveType = AutoFinderAI.Domain.Enums.DriveType;

namespace AutoFinderAI.UnitTests.Crawling;

public class RawListingToCarMapperTests
{
    private static RawListing CreateRawListing() => new(
        ExternalId: "6149575845",
        Url: "https://www.otomoto.pl/oferta/bmw-seria-3-ID6Fp1a2.html",
        Title: "BMW Seria 3 2.0d",
        PublishedAt: new DateTime(2026, 1, 15, 6, 0, 0, DateTimeKind.Utc),
        PriceText: null,
        CurrencyText: null,
        MakeText: "BMW",
        ModelText: "Seria 3",
        YearText: "2007",
        MileageText: "56 377 km",
        FuelTypeText: "Diesel",
        TransmissionText: "Automatyczna",
        EnginePowerText: "163 KM",
        EngineCapacityText: "1 995 cm3",
        BodyTypeText: "Sedan",
        DriveTypeText: "Na tylne koła",
        DoorsText: "4",
        SeatsText: "5",
        ColorText: "Czarny",
        CountryOfOriginText: "Niemcy",
        NoAccidentText: "Tak",
        OriginalOwnerText: "Tak",
        LocationText: null,
        ThumbnailUrl: null);

    [Fact]
    public void TryMap_maps_all_fields_from_a_fully_populated_raw_listing()
    {
        var raw = CreateRawListing();
        var scrapedAt = DateTime.UtcNow;

        var success = RawListingToCarMapper.TryMap(raw, "otomoto.pl", scrapedAt, out var car, out var error);

        success.Should().BeTrue();
        error.Should().BeNull();
        car.Should().NotBeNull();
        car!.Make.Should().Be("BMW");
        car.Model.Should().Be("Seria 3");
        car.ProductionYear.Should().Be(2007);
        car.Mileage.Should().Be(56377);
        car.FuelType.Should().Be(FuelType.Diesel);
        car.Transmission.Should().Be(TransmissionType.Automatic);
        car.EnginePowerHp.Should().Be(163);
        car.EngineCapacityCm3.Should().Be(1995);
        car.BodyType.Should().Be(BodyType.Sedan);
        car.DriveType.Should().Be(DriveType.RearWheel);
        car.Doors.Should().Be(4);
        car.Seats.Should().Be(5);
        car.Color.Should().Be("Czarny");
        car.CountryOfOrigin.Should().Be("Niemcy");
        car.IsDamaged.Should().Be(false);
        car.IsFirstOwner.Should().Be(true);
    }

    [Fact]
    public void TryMap_fails_when_make_or_model_is_missing()
    {
        var raw = CreateRawListing() with { MakeText = null };

        var success = RawListingToCarMapper.TryMap(raw, "otomoto.pl", DateTime.UtcNow, out var car, out var error);

        success.Should().BeFalse();
        car.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryMap_fails_when_year_is_missing_or_invalid()
    {
        var raw = CreateRawListing() with { YearText = null };

        var success = RawListingToCarMapper.TryMap(raw, "otomoto.pl", DateTime.UtcNow, out var car, out var error);

        success.Should().BeFalse();
        car.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryMap_defaults_unknown_enums_and_null_optional_fields_when_text_is_unrecognized()
    {
        var raw = CreateRawListing() with
        {
            FuelTypeText = "nieznane-cos",
            TransmissionText = "nieznane-cos",
            BodyTypeText = "nieznane-cos",
            DriveTypeText = "nieznane-cos",
            MileageText = null,
            DoorsText = null
        };

        var success = RawListingToCarMapper.TryMap(raw, "otomoto.pl", DateTime.UtcNow, out var car, out _);

        success.Should().BeTrue();
        car!.FuelType.Should().Be(FuelType.Unknown);
        car.Transmission.Should().Be(TransmissionType.Unknown);
        car.BodyType.Should().Be(BodyType.Unknown);
        car.DriveType.Should().BeNull();
        car.Mileage.Should().BeNull();
        car.Doors.Should().BeNull();
    }
}
