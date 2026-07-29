using AutoFinderAI.Api.Controllers.Contracts;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Features.Chat.CreateSession;
using AutoFinderAI.Application.Features.Chat.DeleteSession;
using AutoFinderAI.Application.Features.Chat.GetSession;
using AutoFinderAI.Application.Features.Chat.GetSessions;
using AutoFinderAI.Application.Features.Chat.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFinderAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly ISender _sender;

    public ChatController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<CreateSessionResult>> CreateSession(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateSessionCommand(), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<ChatSessionSummaryDto>>> GetSessions(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetSessionsQuery(), cancellationToken));

    [HttpGet("sessions/{id:guid}")]
    public async Task<ActionResult<ChatSessionDetailDto>> GetSession(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSessionQuery(id), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteSessionCommand(id), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpPost("sessions/{id:guid}/messages")]
    public async Task<ActionResult<SendMessageResult>> SendMessage(Guid id, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendMessageCommand(id, request.Content), cancellationToken);
        return this.HandleResult(result);
    }
}
