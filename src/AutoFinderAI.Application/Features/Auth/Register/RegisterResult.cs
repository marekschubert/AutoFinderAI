namespace AutoFinderAI.Application.Features.Auth.Register;

public sealed record RegisterResult(Guid UserId, string Email, string Token);
