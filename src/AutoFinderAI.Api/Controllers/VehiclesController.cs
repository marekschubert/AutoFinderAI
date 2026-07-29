using AutoFinderAI.Api.Controllers.Contracts;
using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Features.Vehicles.GetById;
using AutoFinderAI.Application.Features.Vehicles.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFinderAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly ISender _sender;

    public VehiclesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SearchVehiclesResultItem>>> Search(
        [FromQuery] SearchVehiclesRequest request, CancellationToken cancellationToken)
    {
        var query = new SearchVehiclesQuery(
            request.Make, request.Model, request.MinPrice, request.MaxPrice, request.MinYear,
            request.MaxYear, request.MaxMileage, request.FuelType, request.Transmission,
            request.BodyType, request.MinPowerHp, request.Limit);

        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVehicleByIdQuery(id), cancellationToken);
        return this.HandleResult(result);
    }
}
