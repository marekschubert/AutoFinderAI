using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResult>>;
