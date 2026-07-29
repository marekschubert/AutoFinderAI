using AutoFinderAI.Application.Abstractions;
using MediatR;

namespace AutoFinderAI.Application.Features.Chat.GetSessions;

public sealed record GetSessionsQuery : IRequest<IReadOnlyList<ChatSessionSummaryDto>>;
