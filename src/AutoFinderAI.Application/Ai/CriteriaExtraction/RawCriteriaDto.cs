namespace AutoFinderAI.Application.Ai.CriteriaExtraction;

/// <summary>
/// Loose, fully-nullable DTO matching the wire shape requested from the LLM
/// (<see cref="CriteriaJsonSchema"/>). Deliberately untyped/unsanitized — never used directly by
/// the search pipeline. <see cref="CriteriaSanitizer"/> turns this into a
/// <see cref="AutoFinderAI.Application.Abstractions.VehicleSearchCriteria"/> or rejects it.
/// Unknown JSON members are silently dropped by System.Text.Json's default behavior.
/// </summary>
public sealed class RawCriteriaDto
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public int? MileageMax { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? BodyType { get; set; }
    public int? EnginePowerHpFrom { get; set; }
    public int? EnginePowerHpTo { get; set; }
    public int? SeatsMin { get; set; }
    public bool? ExcludeDamaged { get; set; }
    public string? LocationContains { get; set; }
    public List<string>? Keywords { get; set; }
    public List<string>? SoftPreferences { get; set; }
    public string? SortBy { get; set; }
    public int? Limit { get; set; }
    public string? ClarificationQuestion { get; set; }
    public string? Intro { get; set; }
}
