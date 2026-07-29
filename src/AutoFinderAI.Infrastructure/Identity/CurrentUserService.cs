using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoFinderAI.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AutoFinderAI.Infrastructure.Identity;

/// <summary>Reads the authenticated user id from the JWT `sub` claim of the current HTTP request.
/// JwtSecurityTokenHandler remaps the inbound "sub" claim type to
/// <see cref="ClaimTypes.NameIdentifier"/> by default, so both are checked defensively.</summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var subject = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(subject, out var id) ? id : null;
        }
    }
}
