using System.Globalization;
using System.Text.RegularExpressions;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;
using AutoFinderAI.Domain.Vehicles;
using DriveType = AutoFinderAI.Domain.Enums.DriveType;

namespace AutoFinderAI.Application.Features.Crawling.Mapping;

/// <summary>
/// Maps a source-agnostic <see cref="RawListing"/> (plain scraped strings) into a <see cref="Car"/>
/// domain entity. Culture-safe number parsing, enum whitelist mapping with an Unknown fallback.
/// A missing/unparseable optional field yields null; only a missing Make/Model/Year fails the map
/// (caller decides whether to skip the listing — this method never throws for bad input).
/// </summary>
public static class RawListingToCarMapper
{
    // ASSUMPTION: Location/ThumbnailUrl selectors were not present in the provided otomoto
    // fixtures, so they default to null here pending a confirmed selector map. Price/Currency are
    // parsed from raw.PriceText/CurrencyText (offer-price__number / offer-price__currency); if
    // missing/unparseable they default to 0 PLN.
    // Matches a run of digits that may contain space thousand-separators (e.g. "1 995" in
    // "1 995 cm3", "56 377" in "56 377 km"), stopping before any trailing unit letters so those
    // are never mistaken for extra digits to concatenate.
    private static readonly Regex NumberRegex = new(@"\d[\d\s]*\d|\d", RegexOptions.Compiled);

    public static bool TryMap(RawListing raw, string sourceKey, DateTime scrapedAt, out Car? car, out string? error)
    {
        car = null;

        var make = string.IsNullOrWhiteSpace(raw.MakeText) ? null : raw.MakeText.Trim();
        var model = string.IsNullOrWhiteSpace(raw.ModelText) ? null : raw.ModelText.Trim();
        var year = ParseInt(raw.YearText);

        if (make is null || model is null)
        {
            error = "Missing make/model.";
            return false;
        }

        if (year is null || year < 1900)
        {
            error = "Missing or invalid production year.";
            return false;
        }

        var price = ParseDecimal(raw.PriceText) is { } amount
            ? Money.Create(amount, string.IsNullOrWhiteSpace(raw.CurrencyText) ? "PLN" : raw.CurrencyText.Trim())
            : Money.Create(0, "PLN");

        try
        {
            car = Car.Create(
                sourceKey: sourceKey,
                externalId: raw.ExternalId,
                url: raw.Url,
                title: string.IsNullOrWhiteSpace(raw.Title) ? $"{make} {model}" : raw.Title.Trim(),
                price: price,
                make: make,
                model: model,
                version: null,
                productionYear: year.Value,
                mileage: ParseInt(raw.MileageText),
                fuelType: MapFuelType(raw.FuelTypeText),
                transmission: MapTransmission(raw.TransmissionText),
                enginePowerHp: ParseInt(raw.EnginePowerText),
                engineCapacityCm3: ParseInt(raw.EngineCapacityText),
                location: NullIfBlank(raw.LocationText),
                thumbnailUrl: NullIfBlank(raw.ThumbnailUrl),
                publishedAt: raw.PublishedAt,
                scrapedAt: scrapedAt,
                bodyType: MapBodyType(raw.BodyTypeText),
                doors: ParseInt(raw.DoorsText),
                seats: ParseInt(raw.SeatsText),
                driveType: MapDriveType(raw.DriveTypeText),
                color: NullIfBlank(raw.ColorText),
                isDamaged: MapYesNo(raw.NoAccidentText) is { } noAccident ? !noAccident : null,
                isFirstOwner: MapYesNo(raw.OriginalOwnerText),
                countryOfOrigin: NullIfBlank(raw.CountryOfOriginText));

            error = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static void ApplyUpdate(Car existing, RawListing raw, string sourceKey, DateTime scrapedAt)
    {
        if (!TryMap(raw, sourceKey, scrapedAt, out var fresh, out _) || fresh is null)
        {
            return;
        }

        existing.UpdateFromSource(
            fresh.Title, fresh.Price, fresh.Mileage, fresh.FuelType, fresh.Transmission,
            fresh.EnginePowerHp, fresh.EngineCapacityCm3, fresh.Location, fresh.ThumbnailUrl,
            fresh.PublishedAt, fresh.ScrapedAt, fresh.BodyType, fresh.Doors, fresh.Seats,
            fresh.DriveType, fresh.Color, fresh.IsDamaged, fresh.IsFirstOwner, fresh.CountryOfOrigin);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseInt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var match = NumberRegex.Match(text);
        if (!match.Success)
        {
            return null;
        }

        var digits = new string(match.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var cleaned = Regex.Replace(text, @"[^\d,.]", string.Empty).Replace(",", ".");
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool? MapYesNo(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "tak" => true,
        "nie" => false,
        _ => null
    };

    private static FuelType MapFuelType(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "benzyna" => FuelType.Petrol,
        "diesel" => FuelType.Diesel,
        "lpg" => FuelType.Lpg,
        "benzyna+lpg" => FuelType.Lpg,
        "hybryda" => FuelType.Hybrid,
        "elektryczny" => FuelType.Electric,
        "wodór" => FuelType.Hydrogen,
        _ => FuelType.Unknown
    };

    private static TransmissionType MapTransmission(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "manualna" => TransmissionType.Manual,
        "automatyczna" => TransmissionType.Automatic,
        _ => TransmissionType.Unknown
    };

    private static BodyType MapBodyType(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "sedan" => BodyType.Sedan,
        "hatchback" => BodyType.Hatchback,
        "kombi" => BodyType.Kombi,
        "suv" => BodyType.Suv,
        "coupe" => BodyType.Coupe,
        "kabriolet" => BodyType.Convertible,
        "van" => BodyType.Van,
        "minivan" => BodyType.Van,
        "pickup" => BodyType.Pickup,
        _ => BodyType.Unknown
    };

    private static DriveType? MapDriveType(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "na przednie koła" => DriveType.FrontWheel,
        "na tylne koła" => DriveType.RearWheel,
        "na wszystkie koła (stały)" => DriveType.AllWheel,
        "na wszystkie koła (dołączany automatycznie)" => DriveType.AllWheel,
        "4x4" => DriveType.AllWheel,
        _ => null
    };
}
