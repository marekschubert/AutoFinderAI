namespace AutoFinderAI.Application.Abstractions;

/// <summary>
/// AI-engineer-owned seam (HANDOFF → ai-engineer: implement IResponseComposer). Combines an
/// optional LLM-authored introduction with a deterministic applied-filters summary into the
/// markdown assistant message. Must still produce sensible output when llmIntroduction is null
/// (degraded mode).
/// </summary>
public interface IResponseComposer
{
    string Compose(VehicleSearchCriteria criteria, IReadOnlyList<RankedVehicle> results, string? llmIntroduction);
}
