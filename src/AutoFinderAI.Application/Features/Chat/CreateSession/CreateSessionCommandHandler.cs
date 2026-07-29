using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using AutoFinderAI.Domain.Chat;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.CreateSession;

public sealed class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, Result<CreateSessionResult>>
{
    private const string DefaultTitle = "New chat";

    private static readonly Error NotAuthenticated =
        Error.Unauthorized("Chat.NotAuthenticated", "No authenticated user.");

    private readonly ICurrentUserService _currentUserService;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSessionCommandHandler(
        ICurrentUserService currentUserService,
        IChatSessionRepository chatSessionRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _chatSessionRepository = chatSessionRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateSessionResult>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return NotAuthenticated;
        }

        var session = ChatSession.Start(userId.Value, DefaultTitle, _dateTimeProvider.UtcNow);

        await _chatSessionRepository.AddSessionAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSessionResult(session.Id, session.Title, session.CreatedAt);
    }
}
