using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.CreateSession;

public sealed record CreateSessionCommand : IRequest<Result<CreateSessionResult>>;
