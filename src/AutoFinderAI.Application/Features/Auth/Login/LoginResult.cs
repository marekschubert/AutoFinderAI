namespace AutoFinderAI.Application.Features.Auth.Login;

public sealed record LoginResult(Guid UserId, string Email, string Token);
