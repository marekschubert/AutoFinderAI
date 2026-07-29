using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.SendMessage;

public sealed record SendMessageCommand(Guid SessionId, string Content) : IRequest<Result<SendMessageResult>>;
