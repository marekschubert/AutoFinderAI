using AutoFinderAI.Domain.Chat;
using AutoFinderAI.Domain.Crawling;
using AutoFinderAI.Domain.Users;
using AutoFinderAI.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoFinderAI.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext seam. Entity configurations (IEntityTypeConfiguration, TPH discriminator,
/// indexes, migrations) are added by the backend engineer.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<CrawlRun> CrawlRuns => Set<CrawlRun>();

    public DbSet<User> Users => Set<User>();

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Enables `dotnet ef migrations`/`dotnet ef database update` to construct <see cref="AppDbContext"/>
/// at design time without spinning up the full Api host/DI container. Only used by the EF Core
/// CLI tooling; the running application always resolves <see cref="AppDbContext"/> through
/// <c>AddInfrastructure</c> in <see cref="DependencyInjection"/>.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Data Source=autofinder.db";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
