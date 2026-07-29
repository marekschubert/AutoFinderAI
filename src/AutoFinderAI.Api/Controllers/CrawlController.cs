using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Features.Crawling.GetCrawlRuns;
using AutoFinderAI.Application.Features.Crawling.RunCrawl;
using AutoFinderAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFinderAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/crawl")]
public sealed class CrawlController : ControllerBase
{
    private readonly ISender _sender;

    public CrawlController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("runs")]
    public async Task<ActionResult<RunCrawlResult>> RunCrawl(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RunCrawlCommand(VehicleCategory.Car), cancellationToken);
        return this.HandleResult(result);
    }

    [HttpGet("runs")]
    public async Task<ActionResult<IReadOnlyList<CrawlRunDto>>> GetRuns(
        [FromQuery] int take, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCrawlRunsQuery(take == 0 ? 10 : take), cancellationToken));
}
