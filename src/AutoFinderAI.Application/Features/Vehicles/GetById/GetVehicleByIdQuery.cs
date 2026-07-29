using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using MediatR;

namespace AutoFinderAI.Application.Features.Vehicles.GetById;

public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleDto>>;
