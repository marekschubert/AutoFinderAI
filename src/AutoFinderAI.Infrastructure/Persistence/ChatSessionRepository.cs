using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Chat;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

public sealed class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _dbContext;

    public ChatSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddSessionAsync(ChatSession session, CancellationToken cancellationToken)
        => await _dbContext.ChatSessions.AddAsync(session, cancellationToken);

    public Task<ChatSession?> GetByIdAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
        => _dbContext.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

    public Task<int> CountMessagesAsync(Guid sessionId, CancellationToken cancellationToken)
        => _dbContext.ChatMessages.CountAsync(m => m.SessionId == sessionId, cancellationToken);

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken)
        => await _dbContext.ChatMessages.AddAsync(message, cancellationToken);

    public Task RemoveSessionAsync(ChatSession session, CancellationToken cancellationToken)
    {
        _dbContext.ChatSessions.Remove(session);
        return Task.CompletedTask;
    }
}
