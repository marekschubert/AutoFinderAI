using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Domain.Chat;

public sealed class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = default!;
    public string? CriteriaJson { get; private set; }
    public string? ResultVehicleIdsJson { get; private set; }
    public string? ModelUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ChatMessage()
    {
    }

    private ChatMessage(
        Guid id,
        Guid sessionId,
        MessageRole role,
        string content,
        string? criteriaJson,
        string? resultVehicleIdsJson,
        string? modelUsed,
        DateTime createdAt)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("SessionId is required.", nameof(sessionId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.", nameof(content));
        }

        Id = id;
        SessionId = sessionId;
        Role = role;
        Content = content;
        CriteriaJson = criteriaJson;
        ResultVehicleIdsJson = resultVehicleIdsJson;
        ModelUsed = modelUsed;
        CreatedAt = createdAt;
    }

    public static ChatMessage Create(
        Guid sessionId,
        MessageRole role,
        string content,
        DateTime createdAt,
        string? criteriaJson = null,
        string? resultVehicleIdsJson = null,
        string? modelUsed = null)
        => new(Guid.NewGuid(), sessionId, role, content, criteriaJson, resultVehicleIdsJson, modelUsed, createdAt);
}
