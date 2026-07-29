using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using AutoFinderAI.Domain.Users;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict("Auth.EmailTaken", "An account with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(normalizedEmail, passwordHash, _dateTimeProvider.UtcNow);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.CreateToken(user.Id, user.Email);

        return new RegisterResult(user.Id, user.Email, token);
    }
}
