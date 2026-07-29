using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Vehicles.GetById;

public sealed class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDto>>
{
    private static readonly Error NotFound = Error.NotFound("Vehicle.NotFound", "Vehicle not found.");

    private readonly IVehicleQueries _vehicleQueries;

    public GetVehicleByIdQueryHandler(IVehicleQueries vehicleQueries)
    {
        _vehicleQueries = vehicleQueries;
    }

    public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleQueries.GetByIdAsync(request.Id, cancellationToken);
        return vehicle is null ? NotFound : vehicle;
    }
}
