namespace AutoFinderAI.Application.Abstractions;

/// <summary>Persists aggregate changes. Concrete EF Core implementation lives in Infrastructure.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
