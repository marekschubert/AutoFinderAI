using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Abstractions;

public sealed record ChatSessionSummaryDto(Guid Id, string Title, DateTime CreatedAt, DateTime LastMessageAt);

public sealed record ChatMessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    string? CriteriaJson,
    string? ResultVehicleIdsJson,
    string? ModelUsed,
    DateTime CreatedAt,
    IReadOnlyList<VehicleDto>? Results = null);

public sealed record ChatSessionDetailDto(Guid Id, string Title, DateTime CreatedAt, DateTime LastMessageAt, IReadOnlyList<ChatMessageDto> Messages);

/// <summary>Read-side seam for chat queries. Always filtered by the caller's user id.</summary>
public interface IChatQueries
{
    Task<IReadOnlyList<ChatSessionSummaryDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<ChatSessionDetailDto?> GetSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);
}
