using System.Globalization;
using System.Text;
using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Application.Ai.Composition;

/// <summary>
/// IResponseComposer implementation: combines an optional LLM-authored introduction with a
/// deterministic applied-filters summary and result count into markdown. Degrades gracefully
/// (still produces a sensible reply) when llmIntroduction is null.
/// </summary>
public sealed class MarkdownResponseComposer : IResponseComposer
{
    public string Compose(VehicleSearchCriteria criteria, IReadOnlyList<RankedVehicle> results, string? llmIntroduction)
    {
        var sb = new StringBuilder();

        sb.AppendLine(!string.IsNullOrWhiteSpace(llmIntroduction) ? llmIntroduction.Trim() : DefaultIntro(results.Count));
        sb.AppendLine();

        var filters = DescribeFilters(criteria).ToList();
        if (filters.Count > 0)
        {
            sb.AppendLine("**Applied filters:**");
            foreach (var filter in filters)
            {
                sb.AppendLine($"- {filter}");
            }
            sb.AppendLine();
        }

        sb.Append(results.Count == 0
            ? "No vehicles matched these criteria - try widening your search."
            : $"Found **{results.Count}** matching vehicle(s), best matches first.");

        return sb.ToString();
    }

    private static string DefaultIntro(int count) => count == 0
        ? "I searched using your criteria but couldn't find any matches."
        : "Here is what I found based on your request.";

    private static IEnumerable<string> DescribeFilters(VehicleSearchCriteria criteria)
    {
        if (criteria.Make is not null)
        {
            yield return $"Make: {criteria.Make}";
        }

        if (criteria.Model is not null)
        {
            yield return $"Model: {criteria.Model}";
        }

        if (criteria.MinYear is not null || criteria.MaxYear is not null)
        {
            yield return $"Year: {criteria.MinYear?.ToString() ?? "any"}-{criteria.MaxYear?.ToString() ?? "any"}";
        }

        if (criteria.MinPrice is not null || criteria.MaxPrice is not null)
        {
            yield return $"Price: {Format(criteria.MinPrice) ?? "any"}-{Format(criteria.MaxPrice) ?? "any"}";
        }

        if (criteria.MaxMileage is not null)
        {
            yield return $"Max mileage: {criteria.MaxMileage:N0} km";
        }

        if (criteria.FuelType is not null)
        {
            yield return $"Fuel type: {criteria.FuelType}";
        }

        if (criteria.Transmission is not null)
        {
            yield return $"Transmission: {criteria.Transmission}";
        }

        if (criteria.BodyType is not null)
        {
            yield return $"Body type: {criteria.BodyType}";
        }

        if (criteria.MinPowerHp is not null || criteria.MaxPowerHp is not null)
        {
            yield return $"Power: {criteria.MinPowerHp?.ToString() ?? "any"}-{criteria.MaxPowerHp?.ToString() ?? "any"} hp";
        }

        if (criteria.SeatsMin is not null)
        {
            yield return $"Min seats: {criteria.SeatsMin}";
        }

        if (criteria.ExcludeDamaged is true)
        {
            yield return "Excluding damaged vehicles";
        }

        if (criteria.LocationContains is not null)
        {
            yield return $"Location: {criteria.LocationContains}";
        }

        if (criteria.Keywords is { Count: > 0 })
        {
            yield return $"Keywords: {string.Join(", ", criteria.Keywords)}";
        }

        if (criteria.SoftPreferences is { Count: > 0 })
        {
            yield return $"Preferences: {string.Join(", ", criteria.SoftPreferences)}";
        }

        if (criteria.SortBy != VehicleSortBy.Relevance)
        {
            yield return $"Sorted by: {criteria.SortBy}";
        }
    }

    private static string? Format(decimal? amount) => amount is null ? null : amount.Value.ToString("N0", CultureInfo.InvariantCulture);
}
