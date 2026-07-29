namespace AutoFinderAI.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, string email);
}

/// <summary>Reads the authenticated user id (JWT `sub` claim) for the current request.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
}
