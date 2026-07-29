using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Application.Ai.Ranking;

/// <summary>
/// Deterministic, in-memory ranking over an already SQL-filtered/capped candidate set (never
/// touches the database). Hard criteria are assumed already satisfied by
/// <see cref="IVehicleQueries"/>; scoring here reflects soft preferences and closeness to
/// requested ranges, annotated with human-readable match reasons.
/// </summary>
public sealed class VehicleRanker : IVehicleRanker
{
    public IReadOnlyList<RankedVehicle> Rank(IReadOnlyList<VehicleDto> candidates, VehicleSearchCriteria criteria)
    {
        var scored = candidates.Select(v => Score(v, criteria)).ToList();

        return criteria.SortBy switch
        {
            VehicleSortBy.PriceAsc => scored.OrderBy(r => r.Vehicle.PriceAmount).ToList(),
            VehicleSortBy.PriceDesc => scored.OrderByDescending(r => r.Vehicle.PriceAmount).ToList(),
            VehicleSortBy.YearDesc => scored.OrderByDescending(r => r.Vehicle.ProductionYear).ToList(),
            VehicleSortBy.MileageAsc => scored.OrderBy(r => r.Vehicle.Mileage ?? int.MaxValue).ToList(),
            _ => scored.OrderByDescending(r => r.Score).ThenByDescending(r => r.Vehicle.PublishedAt).ToList()
        };
    }

    private static RankedVehicle Score(VehicleDto vehicle, VehicleSearchCriteria criteria)
    {
        double score = 0;
        var reasons = new List<string>();

        if (!string.IsNullOrWhiteSpace(criteria.Make) && string.Equals(vehicle.Make, criteria.Make, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
            reasons.Add($"Make matches {vehicle.Make}");
        }

        if (!string.IsNullOrWhiteSpace(criteria.Model) && string.Equals(vehicle.Model, criteria.Model, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
            reasons.Add($"Model matches {vehicle.Model}");
        }

        if (criteria.MaxPrice is decimal maxPrice && maxPrice > 0 && vehicle.PriceAmount <= maxPrice)
        {
            var ratio = (double)(vehicle.PriceAmount / maxPrice);
            score += 8 * (1 - ratio);
            reasons.Add($"Within budget ({vehicle.PriceAmount:N0} {vehicle.PriceCurrency})");
        }

        if (criteria.MaxMileage is int maxMileage && maxMileage > 0 && vehicle.Mileage is int mileage && mileage <= maxMileage)
        {
            var ratio = (double)mileage / maxMileage;
            score += 5 * (1 - ratio);
            reasons.Add($"Low mileage ({mileage:N0} km)");
        }

        if (criteria.MinYear is int minYear && vehicle.ProductionYear >= minYear)
        {
            score += 3;
        }

        if (criteria.FuelType is not null && vehicle.FuelType == criteria.FuelType)
        {
            score += 4;
            reasons.Add($"Fuel type matches ({vehicle.FuelType})");
        }

        if (criteria.Transmission is not null && vehicle.Transmission == criteria.Transmission)
        {
            score += 3;
            reasons.Add($"Transmission matches ({vehicle.Transmission})");
        }

        if (criteria.BodyType is not null && vehicle.BodyType == criteria.BodyType)
        {
            score += 3;
            reasons.Add($"Body type matches ({vehicle.BodyType})");
        }

        if (criteria.SeatsMin is int seatsMin && vehicle.Seats is int seats && seats >= seatsMin)
        {
            score += 2;
            reasons.Add($"Seats: {seats}");
        }

        if (criteria.ExcludeDamaged is true && vehicle.IsDamaged is false)
        {
            score += 2;
            reasons.Add("No reported damage");
        }

        if (!string.IsNullOrWhiteSpace(criteria.LocationContains)
            && vehicle.Location is not null
            && vehicle.Location.Contains(criteria.LocationContains, StringComparison.OrdinalIgnoreCase))
        {
            score += 3;
            reasons.Add($"Location matches {criteria.LocationContains}");
        }

        if (criteria.Keywords is { Count: > 0 })
        {
            foreach (var keyword in criteria.Keywords)
            {
                if (vehicle.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                    reasons.Add($"Matches keyword \"{keyword}\"");
                }
            }
        }

        if (criteria.SoftPreferences is { Count: > 0 })
        {
            foreach (var preference in criteria.SoftPreferences)
            {
                if (SoftPreferenceRules.TryEvaluate(preference, vehicle, out var bonus, out var reason))
                {
                    score += bonus;
                    if (reason is not null)
                    {
                        reasons.Add(reason);
                    }
                }
            }
        }

        return new RankedVehicle(vehicle, Math.Round(score, 2), reasons);
    }
}
