using AutoFinderAI.Domain.Vehicles;

namespace AutoFinderAI.Application.Abstractions;

/// <summary>Write-side seam for the crawler upsert pipeline. Kept separate from the read-side
/// <see cref="IVehicleQueries"/> which never returns tracked entities.</summary>
public interface IVehicleRepository
{
    Task<Vehicle?> FindBySourceAsync(string sourceKey, string externalId, CancellationToken cancellationToken);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);

    /// <summary>Stops tracking the given entity. Used to recover from a failed SaveChanges
    /// (e.g. a unique constraint violation) so the rejected entity isn't retried on the next save.</summary>
    void Detach(Vehicle vehicle);
}
