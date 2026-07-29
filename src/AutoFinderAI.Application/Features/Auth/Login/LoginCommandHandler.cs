using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            // Generic error: never reveal whether the email exists.
            return InvalidCredentials;
        }

        var token = _jwtTokenService.CreateToken(user.Id, user.Email);

        return new LoginResult(user.Id, user.Email, token);
    }
}
