using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace AutoFinderAI.Infrastructure.Identity;

/// <summary>Wraps <see cref="PasswordHasher{TUser}"/> so Application code never depends on ASP.NET Identity.</summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string passwordHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
