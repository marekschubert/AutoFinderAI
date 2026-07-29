using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AutoFinderAI.Infrastructure.Ai;

/// <summary>Wire DTOs for the OpenRouter /chat/completions endpoint (OpenAI-compatible shape).</summary>
public sealed class OpenRouterChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenRouterMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("response_format")]
    public OpenRouterResponseFormat? ResponseFormat { get; set; }
}

public sealed class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public sealed class OpenRouterResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("json_schema")]
    public OpenRouterJsonSchema? JsonSchema { get; set; }
}

public sealed class OpenRouterJsonSchema
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("strict")]
    public bool Strict { get; set; }

    [JsonPropertyName("schema")]
    public JsonNode? Schema { get; set; }
}

public sealed class OpenRouterChatResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenRouterChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenRouterUsage? Usage { get; set; }
}

public sealed class OpenRouterChoice
{
    [JsonPropertyName("message")]
    public OpenRouterMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public sealed class OpenRouterUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }
}
