using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Me;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<MeResult>>
{
    private static readonly Error NotAuthenticated =
        Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");

    private static readonly Error UserNotFound =
        Error.NotFound("Auth.UserNotFound", "User not found.");

    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetMeQueryHandler(ICurrentUserService currentUserService, IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result<MeResult>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return NotAuthenticated;
        }

        var user = await _userRepository.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return UserNotFound;
        }

        return new MeResult(user.Id, user.Email, user.CreatedAt);
    }
}
