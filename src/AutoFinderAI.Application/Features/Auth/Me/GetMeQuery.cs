using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Me;

public sealed record GetMeQuery : IRequest<Result<MeResult>>;
