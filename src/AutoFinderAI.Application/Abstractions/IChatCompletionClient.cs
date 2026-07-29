namespace AutoFinderAI.Application.Abstractions;

/// <summary>
/// Request for a single structured completion. <paramref name="JsonSchema"/> carries the raw
/// JSON Schema (as a JSON string) describing the expected response shape; when set, the client
/// asks the provider for strict structured output. <paramref name="Model"/> null means "use the
/// configured default model".
/// </summary>
public sealed record ChatCompletionRequest(
    string SystemPrompt,
    string UserMessage,
    string? Model = null,
    string? JsonSchemaName = null,
    string? JsonSchema = null,
    double Temperature = 0);

public sealed record ChatCompletionResponse(
    string Content,
    string Model,
    int? PromptTokens,
    int? CompletionTokens,
    long DurationMs);

public sealed record ChatCompletionResult(bool Success, ChatCompletionResponse? Response, string? Error);

/// <summary>Seam implemented by the AI engineer over the OpenRouter REST gateway. Never throws for
/// expected failure modes (missing key, transport errors, provider errors) — those are surfaced as
/// a failed <see cref="ChatCompletionResult"/> instead.</summary>
public interface IChatCompletionClient
{
    /// <summary>False when no provider is configured (e.g. no API key) — callers should degrade
    /// instead of attempting a call.</summary>
    bool IsAvailable { get; }

    Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken);
}
