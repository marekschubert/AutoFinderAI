using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Ai.Ranking;

/// <summary>
/// Data-driven, documented rule table mapping free-text soft preferences (English/Polish) onto
/// deterministic vehicle predicates + score bonuses + human-readable match reasons. This is
/// business logic and lives in C#, not the prompt.
/// </summary>
public static class SoftPreferenceRules
{
    private static readonly HashSet<string> PremiumMakes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BMW", "Audi", "Mercedes-Benz", "Mercedes", "Porsche", "Lexus", "Jaguar", "Land Rover", "Volvo"
    };

    public static bool TryEvaluate(string rawPreference, VehicleDto vehicle, out double bonus, out string? reason)
    {
        bonus = 0;
        reason = null;

        switch (Normalize(rawPreference))
        {
            case "family" or "rodzinny" or "rodzinne":
                if (vehicle.BodyType is BodyType.Kombi or BodyType.Suv or BodyType.Van
                    && (vehicle.Seats is null || vehicle.Seats >= 5))
                {
                    bonus = 8;
                    reason = "Family-friendly body type with enough seats";
                    return true;
                }
                return false;

            case "reliable" or "niezawodny" or "niezawodne":
                if ((vehicle.Mileage is null || vehicle.Mileage < 150_000) && vehicle.ProductionYear >= 2015)
                {
                    bonus = 8;
                    reason = "Recent model with moderate mileage";
                    return true;
                }
                return false;

            case "economical" or "oszczedny" or "oszczedne":
                if (vehicle.FuelType is FuelType.Hybrid or FuelType.Electric or FuelType.Lpg)
                {
                    bonus = 6;
                    reason = $"Economical fuel type ({vehicle.FuelType})";
                    return true;
                }
                return false;

            case "luxury" or "premium" or "luksusowy" or "luksusowe":
                if (PremiumMakes.Contains(vehicle.Make))
                {
                    bonus = 6;
                    reason = $"Premium make ({vehicle.Make})";
                    return true;
                }
                return false;

            case "sporty" or "sportowy" or "sportowe":
                if (vehicle.BodyType is BodyType.Coupe or BodyType.Convertible || vehicle.EnginePowerHp >= 200)
                {
                    bonus = 6;
                    reason = "Sporty body type or high engine power";
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant()
        .Replace("ą", "a").Replace("ę", "e").Replace("ó", "o").Replace("ł", "l")
        .Replace("ś", "s").Replace("ż", "z").Replace("ź", "z").Replace("ć", "c").Replace("ń", "n");
}
