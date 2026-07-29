using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

/// <summary>
/// Read-side vehicle queries. Hard filters and sorting run in SQL; results are capped with
/// <c>Take(candidateCap)</c> before projecting to <see cref="VehicleDto"/>, so the whole table is
/// never materialised.
/// </summary>
public sealed class VehicleQueries : IVehicleQueries
{
    private readonly AppDbContext _dbContext;

    public VehicleQueries(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VehicleDto>> SearchAsync(VehicleSearchCriteria criteria, int candidateCap, CancellationToken cancellationToken)
    {
        var query = _dbContext.Vehicles.AsNoTracking().OfType<Car>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Make))
        {
            query = query.Where(v => v.Make == criteria.Make);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Model))
        {
            query = query.Where(v => v.Model == criteria.Model);
        }

        if (criteria.MinPrice is not null)
        {
            query = query.Where(v => v.Price.Amount >= criteria.MinPrice);
        }

        if (criteria.MaxPrice is not null)
        {
            query = query.Where(v => v.Price.Amount <= criteria.MaxPrice);
        }

        if (criteria.MinYear is not null)
        {
            query = query.Where(v => v.ProductionYear >= criteria.MinYear);
        }

        if (criteria.MaxYear is not null)
        {
            query = query.Where(v => v.ProductionYear <= criteria.MaxYear);
        }

        if (criteria.MaxMileage is not null)
        {
            query = query.Where(v => v.Mileage == null || v.Mileage <= criteria.MaxMileage);
        }

        if (criteria.FuelType is not null)
        {
            query = query.Where(v => v.FuelType == criteria.FuelType);
        }

        if (criteria.Transmission is not null)
        {
            query = query.Where(v => v.Transmission == criteria.Transmission);
        }

        if (criteria.BodyType is not null)
        {
            query = query.Where(v => v.BodyType == criteria.BodyType);
        }

        if (criteria.MinPowerHp is not null)
        {
            query = query.Where(v => v.EnginePowerHp == null || v.EnginePowerHp >= criteria.MinPowerHp);
        }

        if (criteria.MaxPowerHp is not null)
        {
            query = query.Where(v => v.EnginePowerHp == null || v.EnginePowerHp <= criteria.MaxPowerHp);
        }

        if (criteria.SeatsMin is not null)
        {
            query = query.Where(v => v.Seats == null || v.Seats >= criteria.SeatsMin);
        }

        if (criteria.ExcludeDamaged is true)
        {
            query = query.Where(v => v.IsDamaged != true);
        }

        if (!string.IsNullOrWhiteSpace(criteria.LocationContains))
        {
            var locationPattern = $"%{criteria.LocationContains}%";
            query = query.Where(v => v.Location != null && EF.Functions.Like(v.Location, locationPattern));
        }

        if (criteria.Keywords is { Count: > 0 })
        {
            foreach (var keyword in criteria.Keywords)
            {
                var pattern = $"%{keyword}%";
                query = query.Where(v => EF.Functions.Like(v.Title, pattern)
                    || EF.Functions.Like(v.Make, pattern)
                    || EF.Functions.Like(v.Model, pattern));
            }
        }

        var candidates = await query
            .OrderByDescending(v => v.PublishedAt)
            .Take(candidateCap)
            .Select(v => ToDto(v))
            .ToListAsync(cancellationToken);

        return candidates;
    }

    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Vehicles.AsNoTracking().OfType<Car>()
            .Where(v => v.Id == id)
            .Select(v => ToDto(v))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleDto>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<VehicleDto>();
        }

        return await _dbContext.Vehicles.AsNoTracking().OfType<Car>()
            .Where(v => ids.Contains(v.Id))
            .Select(v => ToDto(v))
            .ToListAsync(cancellationToken);
    }

    private static VehicleDto ToDto(Car v) => new(
        v.Id, v.Url, v.Title, v.Price.Amount, v.Price.Currency, v.Make, v.Model, v.Version,
        v.ProductionYear, v.Mileage, v.FuelType, v.Transmission, v.EnginePowerHp, v.EngineCapacityCm3,
        v.Location, v.ThumbnailUrl, v.PublishedAt, v.BodyType, v.Doors, v.Seats, v.DriveType, v.Color,
        v.IsDamaged, v.IsFirstOwner, v.CountryOfOrigin);
}
