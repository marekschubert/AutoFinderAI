using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password) : IRequest<Result<RegisterResult>>;
