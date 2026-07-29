using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.GetSession;

public sealed class GetSessionQueryHandler : IRequestHandler<GetSessionQuery, Result<ChatSessionDetailDto>>
{
    private static readonly Error NotAuthenticated =
        Error.Unauthorized("Chat.NotAuthenticated", "No authenticated user.");

    private static readonly Error NotFound =
        Error.NotFound("Chat.SessionNotFound", "Chat session not found.");

    private readonly ICurrentUserService _currentUserService;
    private readonly IChatQueries _chatQueries;

    public GetSessionQueryHandler(ICurrentUserService currentUserService, IChatQueries chatQueries)
    {
        _currentUserService = currentUserService;
        _chatQueries = chatQueries;
    }

    public async Task<Result<ChatSessionDetailDto>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return NotAuthenticated;
        }

        var session = await _chatQueries.GetSessionAsync(request.SessionId, userId.Value, cancellationToken);
        return session is null ? NotFound : session;
    }
}
