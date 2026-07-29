namespace AutoFinderAI.Application.Abstractions;

/// <summary>
/// Application-layer view of the AI-engineer-owned numeric knobs (backed by Infrastructure's
/// <c>AiOptions</c>), so handlers can stay config-driven without Application depending on
/// Infrastructure.
/// </summary>
public interface IAiSearchOptions
{
    /// <summary>Max rows pulled from SQL before in-memory ranking (candidate cap).</summary>
    int MaxCandidates { get; }

    /// <summary>Default number of results returned when the caller/criteria did not request one.</summary>
    int DefaultLimit { get; }

    /// <summary>Upper bound a requested/extracted limit is clamped to.</summary>
    int MaxLimit { get; }

    /// <summary>Max repair-retry attempts when the LLM returns malformed structured output.</summary>
    int MaxRepairRetries { get; }
}
