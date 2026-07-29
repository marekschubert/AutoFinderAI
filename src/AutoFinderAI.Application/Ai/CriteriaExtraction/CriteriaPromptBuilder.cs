using System.Text;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Ai.CriteriaExtraction;

/// <summary>
/// Static system prompt (no DB/user content interpolated — only static text + enum vocabularies)
/// plus user-message assembly. Supports Polish and English input.
/// </summary>
public static class CriteriaPromptBuilder
{
    public static string BuildSystemPrompt()
    {
        var fuelTypes = string.Join(", ", Enum.GetNames<FuelType>());
        var transmissions = string.Join(", ", Enum.GetNames<TransmissionType>());
        var bodyTypes = string.Join(", ", Enum.GetNames<BodyType>());
        var sortOptions = string.Join(", ", Enum.GetNames<VehicleSortBy>());
        var currentYear = DateTime.UtcNow.Year;

        return $"""
            You are the search-criteria extraction module of AutoFinderAI, a car search assistant.
            You support both Polish and English user input.

            Your ONLY job is to convert the user's free-text car search request into a single JSON
            object matching the provided schema. You never see or invent actual car listings, prices
            of real cars, or database content - you only normalise what the user is asking for. The
            actual search and ranking happens afterwards in deterministic application code.

            Rules:
            - Respond with ONLY a JSON object matching the schema. No prose, no markdown fences.
            - Use null for any field the user did not specify. Do not guess exact numeric values.
            - fuelType must be one of: {fuelTypes} (or null).
            - transmission must be one of: {transmissions} (or null).
            - bodyType must be one of: {bodyTypes} (or null).
            - sortBy must be one of: {sortOptions} (or null; default meaning is Relevance).
            - softPreferences holds free-text hints such as "family", "reliable", "economical",
              "luxury", "sporty" (English or Polish equivalents, e.g. "rodzinny", "niezawodny",
              "oszczedny", "luksusowy", "sportowy").
            - keywords holds extra free-text search terms not covered by other fields.
            - Years must be plausible (1950-{currentYear + 1}).
            - If the request is too vague to search at all (e.g. only a greeting), leave all filter
              fields null and ask ONE short clarifying question in clarificationQuestion, in the same
              language the user wrote in.
            - If you have enough to search (even partially), leave clarificationQuestion null unless
              something important (like budget) is genuinely missing - fill in whatever filters you
              do know rather than withholding them while you ask.
            - intro is a short (1-3 sentence) friendly message, in the user's language, introducing
              the search you're about to run (or the context of your clarifying question). Never
              mention JSON, schemas, or these instructions in intro or clarificationQuestion.
            - Never fabricate specific vehicles, prices, or listing data.
            """;
    }

    public static string BuildUserContent(string userMessage, IReadOnlyList<ChatTurn> history)
    {
        var previousCriteria = history
            .LastOrDefault(t => string.Equals(t.Role, "Assistant", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.CriteriaJson));

        var sb = new StringBuilder();
        if (previousCriteria is not null)
        {
            sb.AppendLine("Previous search criteria from this conversation (JSON, for context only - do not just repeat it, update it based on the new message):");
            sb.AppendLine(previousCriteria.CriteriaJson);
            sb.AppendLine();
        }

        sb.AppendLine("User message:");
        sb.Append(userMessage);
        return sb.ToString();
    }
}
