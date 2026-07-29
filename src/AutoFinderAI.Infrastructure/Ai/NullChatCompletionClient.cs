using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Infrastructure.Ai;

/// <summary>Used when no OpenRouter API key is configured. Never calls out to the network; always
/// returns a typed "AI unavailable" failure so callers (ICriteriaExtractor) can degrade gracefully.</summary>
public sealed class NullChatCompletionClient : IChatCompletionClient
{
    public bool IsAvailable => false;

    public Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new ChatCompletionResult(false, null, "AI is unavailable: no OpenRouter API key configured."));
}
