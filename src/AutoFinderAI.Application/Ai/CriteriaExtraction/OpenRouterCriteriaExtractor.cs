using System.Text.Json;
using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Application.Ai.CriteriaExtraction;

/// <summary>
/// ICriteriaExtractor implementation: builds the static prompt, calls IChatCompletionClient for a
/// structured JSON response, deserializes/sanitizes/validates it, retries once (with the parse
/// error appended) on malformed JSON, and always returns a typed result - never throws for
/// expected failure modes.
/// </summary>
public sealed class OpenRouterCriteriaExtractor : ICriteriaExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IChatCompletionClient _chatClient;
    private readonly IAiSearchOptions _options;

    public OpenRouterCriteriaExtractor(IChatCompletionClient chatClient, IAiSearchOptions options)
    {
        _chatClient = chatClient;
        _options = options;
    }

    public async Task<CriteriaExtractionResult> ExtractAsync(
        string userMessage, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken)
    {
        if (!_chatClient.IsAvailable)
        {
            return new CriteriaExtractionResult(
                Criteria: null,
                ClarificationQuestion: "The AI search assistant is currently unavailable (no API key configured). "
                    + "Please try the structured search on the Vehicles page instead.",
                ModelUsed: null,
                Intro: null);
        }

        var systemPrompt = CriteriaPromptBuilder.BuildSystemPrompt();
        var userContent = CriteriaPromptBuilder.BuildUserContent(userMessage, history);

        string? lastError = null;
        var maxAttempts = Math.Max(1, _options.MaxRepairRetries + 1);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var prompt = attempt == 0
                ? userContent
                : $"{userContent}\n\nYour previous response was invalid: {lastError}. "
                    + "Reply again with ONLY a valid JSON object matching the schema, no commentary.";

            var request = new ChatCompletionRequest(
                SystemPrompt: systemPrompt,
                UserMessage: prompt,
                JsonSchemaName: CriteriaJsonSchema.Name,
                JsonSchema: CriteriaJsonSchema.Schema);

            var result = await _chatClient.CompleteAsync(request, cancellationToken);

            if (!result.Success || result.Response is null)
            {
                lastError = result.Error ?? "empty response from the model";
                continue;
            }

            RawCriteriaDto? raw;
            try
            {
                raw = JsonSerializer.Deserialize<RawCriteriaDto>(ExtractJson(result.Response.Content), JsonOptions);
            }
            catch (JsonException ex)
            {
                lastError = ex.Message;
                continue;
            }

            if (raw is null)
            {
                lastError = "the model returned a null JSON payload";
                continue;
            }

            var (criteria, intro) = CriteriaSanitizer.Sanitize(raw, _options);
            var clarification = string.IsNullOrWhiteSpace(raw.ClarificationQuestion) ? null : raw.ClarificationQuestion.Trim();

            if (criteria is null && clarification is null)
            {
                clarification = "Could you share a few more details about the car you're looking for "
                    + "(make, budget, or body type)?";
            }

            return new CriteriaExtractionResult(criteria, clarification, result.Response.Model, intro);
        }

        return new CriteriaExtractionResult(
            Criteria: null,
            ClarificationQuestion: "Sorry, I couldn't process that request right now. Could you try rephrasing it?",
            ModelUsed: null,
            Intro: null);
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }
}
