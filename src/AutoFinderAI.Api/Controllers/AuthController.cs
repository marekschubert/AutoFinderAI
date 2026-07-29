using AutoFinderAI.Api.Controllers.Contracts;
using AutoFinderAI.Application.Features.Auth.Login;
using AutoFinderAI.Application.Features.Auth.Me;
using AutoFinderAI.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFinderAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResult>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterCommand(request.Email, request.Password), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResult>> Me(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMeQuery(), cancellationToken);
        return this.HandleResult(result);
    }
}
