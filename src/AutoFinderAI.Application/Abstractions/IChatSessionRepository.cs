using AutoFinderAI.Domain.Chat;

namespace AutoFinderAI.Application.Abstractions;

/// <summary>Write-side seam for chat commands. Every method is scoped by the owning user.</summary>
public interface IChatSessionRepository
{
    Task AddSessionAsync(ChatSession session, CancellationToken cancellationToken);

    Task<ChatSession?> GetByIdAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

    Task<int> CountMessagesAsync(Guid sessionId, CancellationToken cancellationToken);

    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken);

    Task RemoveSessionAsync(ChatSession session, CancellationToken cancellationToken);
}
