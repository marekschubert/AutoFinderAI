namespace AutoFinderAI.Infrastructure.Options;

/// <summary>
/// AI subsystem configuration. <see cref="Models"/>/<see cref="DefaultModel"/> live in
/// appsettings.json (edit in one place, no rebuild needed - just an app restart). <see cref="ApiKey"/>
/// is never stored in appsettings.json; it is populated from the OPENROUTER_API_KEY environment
/// variable (sourced from a git-ignored .env file locally, or real environment/secret injection in
/// deployment) - see DependencyInjection.AddInfrastructure.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>OpenRouter API key. Populated from OPENROUTER_API_KEY, never from appsettings.json.</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>Allow-listed OpenRouter model ids. Keep this to a handful of free (":free") models
    /// by default to avoid incurring cost; edit appsettings.json to change without rebuilding.</summary>
    public IReadOnlyList<string> Models { get; set; } = Array.Empty<string>();

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Max SQL-filtered candidates handed to IVehicleRanker before Take(limit).</summary>
    public int MaxCandidates { get; set; } = 200;

    /// <summary>Default number of results when the extracted/requested limit is unset.</summary>
    public int DefaultLimit { get; set; } = 10;

    /// <summary>Upper bound any requested/extracted limit is clamped to.</summary>
    public int MaxLimit { get; set; } = 50;

    /// <summary>Suspended (see docs/TASKS.md "Degraded mode"): keyword fallback search is not
    /// implemented; when AI is unavailable the assistant just reports degraded mode.</summary>
    public bool AllowKeywordFallback { get; set; }

    public double Temperature { get; set; }

    /// <summary>Max repair-retry attempts when the model returns malformed/invalid JSON.</summary>
    public int MaxRepairRetries { get; set; } = 1;

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    /// <summary>DefaultModel if it's in the allow-list, otherwise the first configured model.</summary>
    public string EffectiveDefaultModel => Models.Contains(DefaultModel) ? DefaultModel : Models.FirstOrDefault() ?? DefaultModel;
}
