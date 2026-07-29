using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Api.Controllers;

/// <summary>Read-only status/catalogue endpoints for the AI subsystem. Never exposes the API key.</summary>
[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly AiOptions _options;
    private readonly IChatCompletionClient _chatCompletionClient;

    public AiController(IOptions<AiOptions> options, IChatCompletionClient chatCompletionClient)
    {
        _options = options.Value;
        _chatCompletionClient = chatCompletionClient;
    }

    [HttpGet("status")]
    public IActionResult GetStatus() => Ok(new
    {
        available = _chatCompletionClient.IsAvailable,
        defaultModel = _options.EffectiveDefaultModel,
        allowKeywordFallback = _options.AllowKeywordFallback
    });

    [HttpGet("models")]
    public IActionResult GetModels() => Ok(new
    {
        defaultModel = _options.EffectiveDefaultModel,
        models = _options.Models
    });
}
