using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _dbContext;

    public VehicleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Vehicle?> FindBySourceAsync(string sourceKey, string externalId, CancellationToken cancellationToken)
        => _dbContext.Vehicles.FirstOrDefaultAsync(
            v => v.SourceKey == sourceKey && v.ExternalId == externalId, cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
        => await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);

    public void Detach(Vehicle vehicle)
        => _dbContext.Entry(vehicle).State = EntityState.Detached;
}
