namespace AutoFinderAI.Application.Abstractions;

/// <summary><paramref name="CriteriaJson"/> is the sanitized criteria persisted for a prior
/// assistant turn (if any), reused as compact context for follow-up messages (max 1 turn back).</summary>
public sealed record ChatTurn(string Role, string Content, string? CriteriaJson = null);

/// <summary><paramref name="Intro"/> is the LLM-authored 1-3 sentence introduction, consumed by
/// <see cref="IResponseComposer"/>; null in degraded mode or when no search was run.</summary>
public sealed record CriteriaExtractionResult(
    VehicleSearchCriteria? Criteria,
    string? ClarificationQuestion,
    string? ModelUsed,
    string? Intro = null);

/// <summary>
/// AI-engineer-owned seam (HANDOFF → ai-engineer: implement ICriteriaExtractor over OpenRouter).
/// Converts free-text chat input into a sanitized <see cref="VehicleSearchCriteria"/>, or a
/// clarification question when the request is ambiguous. Never queries the database.
/// </summary>
public interface ICriteriaExtractor
{
    Task<CriteriaExtractionResult> ExtractAsync(string userMessage, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken);
}
