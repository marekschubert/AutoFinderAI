using AutoFinderAI.Application.Abstractions;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.GetSessions;

public sealed class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, IReadOnlyList<ChatSessionSummaryDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatQueries _chatQueries;

    public GetSessionsQueryHandler(ICurrentUserService currentUserService, IChatQueries chatQueries)
    {
        _currentUserService = currentUserService;
        _chatQueries = chatQueries;
    }

    public async Task<IReadOnlyList<ChatSessionSummaryDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<ChatSessionSummaryDto>();
        }

        return await _chatQueries.GetSessionsAsync(userId.Value, cancellationToken);
    }
}
