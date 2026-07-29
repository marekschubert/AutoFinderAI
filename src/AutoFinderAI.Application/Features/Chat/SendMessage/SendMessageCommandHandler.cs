using System.Text.Json;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using AutoFinderAI.Domain.Chat;
using AutoFinderAI.Domain.Enums;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.SendMessage;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<SendMessageResult>>
{
    private const int TitleMaxLength = 60;

    private static readonly Error NotAuthenticated =
        Error.Unauthorized("Chat.NotAuthenticated", "No authenticated user.");

    private static readonly Error NotFound =
        Error.NotFound("Chat.SessionNotFound", "Chat session not found.");

    private readonly ICurrentUserService _currentUserService;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IChatQueries _chatQueries;
    private readonly ICriteriaExtractor _criteriaExtractor;
    private readonly IVehicleQueries _vehicleQueries;
    private readonly IVehicleRanker _vehicleRanker;
    private readonly IResponseComposer _responseComposer;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiSearchOptions _aiSearchOptions;

    public SendMessageCommandHandler(
        ICurrentUserService currentUserService,
        IChatSessionRepository chatSessionRepository,
        IChatQueries chatQueries,
        ICriteriaExtractor criteriaExtractor,
        IVehicleQueries vehicleQueries,
        IVehicleRanker vehicleRanker,
        IResponseComposer responseComposer,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IAiSearchOptions aiSearchOptions)
    {
        _currentUserService = currentUserService;
        _chatSessionRepository = chatSessionRepository;
        _chatQueries = chatQueries;
        _criteriaExtractor = criteriaExtractor;
        _vehicleQueries = vehicleQueries;
        _vehicleRanker = vehicleRanker;
        _responseComposer = responseComposer;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _aiSearchOptions = aiSearchOptions;
    }

    public async Task<Result<SendMessageResult>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
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

        var isFirstMessage = await _chatSessionRepository.CountMessagesAsync(session.Id, cancellationToken) == 0;
        var now = _dateTimeProvider.UtcNow;

        var userMessage = ChatMessage.Create(session.Id, MessageRole.User, request.Content, now);
        await _chatSessionRepository.AddMessageAsync(userMessage, cancellationToken);

        if (isFirstMessage)
        {
            session.Rename(Truncate(request.Content, TitleMaxLength));
        }

        var history = (await _chatQueries.GetSessionAsync(session.Id, userId.Value, cancellationToken))
            ?.Messages.Select(m => new ChatTurn(m.Role.ToString(), m.Content, m.CriteriaJson)).ToList()
            ?? new List<ChatTurn>();

        var extraction = await _criteriaExtractor.ExtractAsync(request.Content, history, cancellationToken);

        string assistantContent;
        string? criteriaJson = null;
        string? resultVehicleIdsJson = null;
        IReadOnlyList<VehicleDto> results = Array.Empty<VehicleDto>();

        if (extraction.Criteria is null)
        {
            assistantContent = extraction.ClarificationQuestion
                ?? "Could you provide more details about the car you're looking for?";
        }
        else
        {
            var candidates = await _vehicleQueries.SearchAsync(extraction.Criteria, _aiSearchOptions.MaxCandidates, cancellationToken);
            var limit = extraction.Criteria.Limit ?? _aiSearchOptions.DefaultLimit;
            var ranked = _vehicleRanker.Rank(candidates, extraction.Criteria).Take(limit).ToList();

            assistantContent = _responseComposer.Compose(extraction.Criteria, ranked, extraction.Intro);
            criteriaJson = JsonSerializer.Serialize(extraction.Criteria);
            results = ranked.Select(r => r.Vehicle).ToList();
            resultVehicleIdsJson = JsonSerializer.Serialize(results.Select(r => r.Id));
        }

        var assistantMessage = ChatMessage.Create(
            session.Id, MessageRole.Assistant, assistantContent, _dateTimeProvider.UtcNow,
            criteriaJson, resultVehicleIdsJson, extraction.ModelUsed);

        await _chatSessionRepository.AddMessageAsync(assistantMessage, cancellationToken);
        session.Touch(_dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assistantDto = new ChatMessageDto(
            assistantMessage.Id, assistantMessage.Role, assistantMessage.Content,
            assistantMessage.CriteriaJson, assistantMessage.ResultVehicleIdsJson, assistantMessage.ModelUsed,
            assistantMessage.CreatedAt);

        return new SendMessageResult(assistantDto, extraction.Criteria, results, extraction.ClarificationQuestion);
    }

    private static string Truncate(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength].TrimEnd() + "…";
    }
}
