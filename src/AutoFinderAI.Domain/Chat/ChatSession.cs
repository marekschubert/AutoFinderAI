namespace AutoFinderAI.Domain.Chat;

public sealed class ChatSession
{
    private readonly List<ChatMessage> _messages = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastMessageAt { get; private set; }
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatSession()
    {
    }

    private ChatSession(Guid id, Guid userId, string title, DateTime createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Id = id;
        UserId = userId;
        Title = title;
        CreatedAt = createdAt;
        LastMessageAt = createdAt;
    }

    public static ChatSession Start(Guid userId, string title, DateTime createdAt)
        => new(Guid.NewGuid(), userId, title, createdAt);

    public void Touch(DateTime at) => LastMessageAt = at;

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title;
    }
}
