namespace AutoFinderAI.Application.Features.Auth.Me;

public sealed record MeResult(Guid UserId, string Email, DateTime CreatedAt);
