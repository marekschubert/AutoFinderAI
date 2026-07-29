using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.DeleteSession;

public sealed class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, Result>
{
    private static readonly Error NotAuthenticated =
        Error.Unauthorized("Chat.NotAuthenticated", "No authenticated user.");

    private static readonly Error NotFound =
        Error.NotFound("Chat.SessionNotFound", "Chat session not found.");

    private readonly ICurrentUserService _currentUserService;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSessionCommandHandler(
        ICurrentUserService currentUserService,
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return NotAuthenticated;
        }

        var session = await _chatSessionRepository.GetByIdAsync(request.SessionId, userId.Value, cancellationToken);
        if (session is null)
        {
            return NotFound;
        }

        await _chatSessionRepository.RemoveSessionAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
