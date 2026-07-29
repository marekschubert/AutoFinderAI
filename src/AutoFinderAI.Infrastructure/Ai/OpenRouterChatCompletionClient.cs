using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Infrastructure.Ai;

/// <summary>
/// IChatCompletionClient implementation over the OpenRouter REST gateway. Requests strict
/// json_schema structured output first, falls back to json_object if the model rejects strict
/// schemas, retries transient 429/5xx errors with backoff, and logs model/token/latency.
/// </summary>
public sealed class OpenRouterChatCompletionClient : IChatCompletionClient
{
    private const int MaxTransientRetries = 3;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<OpenRouterChatCompletionClient> _logger;

    public OpenRouterChatCompletionClient(HttpClient httpClient, IOptions<AiOptions> options, ILogger<OpenRouterChatCompletionClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsAvailable => _options.HasApiKey;

    public async Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            return new ChatCompletionResult(false, null, "OpenRouter API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.EffectiveDefaultModel : request.Model;
        if (string.IsNullOrWhiteSpace(model))
        {
            return new ChatCompletionResult(false, null, "No AI model is configured (Ai:Models is empty).");
        }

        var formats = BuildResponseFormatAttempts(request);
        string? lastError = null;

        foreach (var format in formats)
        {
            var payload = new OpenRouterChatRequest
            {
                Model = model,
                Temperature = request.Temperature,
                ResponseFormat = format,
                Messages =
                {
                    new OpenRouterMessage { Role = "system", Content = request.SystemPrompt },
                    new OpenRouterMessage { Role = "user", Content = request.UserMessage }
                }
            };

            for (var attempt = 0; attempt < MaxTransientRetries; attempt++)
            {
                var stopwatch = Stopwatch.StartNew();
                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.PostAsJsonAsync("chat/completions", payload, JsonOptions, cancellationToken);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
                {
                    lastError = $"transport error: {ex.Message}";
                    _logger.LogWarning(ex, "OpenRouter transport error (attempt {Attempt})", attempt + 1);
                    await DelayBeforeRetry(attempt, cancellationToken);
                    continue;
                }

                using (response)
                {
                    if (response.StatusCode == (HttpStatusCode)429 || (int)response.StatusCode >= 500)
                    {
                        lastError = $"transient HTTP {(int)response.StatusCode}";
                        _logger.LogWarning("OpenRouter returned {StatusCode} (attempt {Attempt})", (int)response.StatusCode, attempt + 1);
                        await DelayBeforeRetry(attempt, cancellationToken);
                        continue;
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest && format?.Type == "json_schema")
                    {
                        // Model likely doesn't support strict json_schema - fall back to the next format.
                        lastError = "model rejected strict json_schema response_format";
                        break;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning("OpenRouter error {StatusCode}: {Body}", (int)response.StatusCode, body);
                        return new ChatCompletionResult(false, null, $"OpenRouter error {(int)response.StatusCode}: {body}");
                    }

                    var parsed = await response.Content.ReadFromJsonAsync<OpenRouterChatResponse>(JsonOptions, cancellationToken);
                    var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        return new ChatCompletionResult(false, null, "OpenRouter returned an empty completion.");
                    }

                    stopwatch.Stop();
                    var usedModel = parsed?.Model ?? model;
                    _logger.LogInformation(
                        "OpenRouter completion: model={Model} promptTokens={PromptTokens} completionTokens={CompletionTokens} durationMs={DurationMs}",
                        usedModel, parsed?.Usage?.PromptTokens, parsed?.Usage?.CompletionTokens, stopwatch.ElapsedMilliseconds);

                    return new ChatCompletionResult(true, new ChatCompletionResponse(
                        content, usedModel, parsed?.Usage?.PromptTokens, parsed?.Usage?.CompletionTokens, stopwatch.ElapsedMilliseconds), null);
                }
            }
        }

        return new ChatCompletionResult(false, null, $"OpenRouter request failed: {lastError ?? "unknown error"}");
    }

    private static IEnumerable<OpenRouterResponseFormat?> BuildResponseFormatAttempts(ChatCompletionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.JsonSchema))
        {
            yield return null;
            yield break;
        }

        yield return new OpenRouterResponseFormat
        {
            Type = "json_schema",
            JsonSchema = new OpenRouterJsonSchema
            {
                Name = request.JsonSchemaName ?? "structured_response",
                Strict = true,
                Schema = JsonNode.Parse(request.JsonSchema)
            }
        };

        yield return new OpenRouterResponseFormat { Type = "json_object" };
    }

    private static Task DelayBeforeRetry(int attempt, CancellationToken cancellationToken)
        => Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), cancellationToken);
}
