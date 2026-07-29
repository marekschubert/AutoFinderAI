using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Application.Features.Chat.SendMessage;

public sealed record SendMessageResult(
    ChatMessageDto AssistantMessage,
    VehicleSearchCriteria? Criteria,
    IReadOnlyList<VehicleDto> Results,
    string? ClarificationQuestion);
