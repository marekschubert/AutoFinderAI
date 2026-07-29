namespace AutoFinderAI.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    private User(Guid id, string email, string passwordHash, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));
        }

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public static User Create(string email, string passwordHash, DateTime createdAt)
        => new(Guid.NewGuid(), email, passwordHash, createdAt);
}
