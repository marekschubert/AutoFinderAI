namespace AutoFinderAI.Application.Abstractions;

public sealed record RankedVehicle(VehicleDto Vehicle, double Score, IReadOnlyList<string> MatchReasons);

/// <summary>
/// AI-engineer-owned seam (HANDOFF → ai-engineer: implement IVehicleRanker with deterministic,
/// weighted scoring). Ranks an already SQL-filtered/capped candidate set; never touches the database.
/// </summary>
public interface IVehicleRanker
{
    IReadOnlyList<RankedVehicle> Rank(IReadOnlyList<VehicleDto> candidates, VehicleSearchCriteria criteria);
}
