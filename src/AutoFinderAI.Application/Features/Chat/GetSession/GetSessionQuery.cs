using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.GetSession;

public sealed record GetSessionQuery(Guid SessionId) : IRequest<Result<ChatSessionDetailDto>>;
