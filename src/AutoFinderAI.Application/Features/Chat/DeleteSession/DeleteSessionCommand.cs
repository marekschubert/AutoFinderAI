using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.DeleteSession;

public sealed record DeleteSessionCommand(Guid SessionId) : IRequest<Result>;
