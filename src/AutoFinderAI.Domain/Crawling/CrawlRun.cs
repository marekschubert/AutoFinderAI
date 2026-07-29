using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Domain.Crawling;

public sealed class CrawlRun
{
    public Guid Id { get; private set; }
    public string SourceKey { get; private set; } = default!;
    public VehicleCategory Category { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public CrawlStatus Status { get; private set; }
    public int ItemsFound { get; private set; }
    public int ItemsSaved { get; private set; }
    public string? Error { get; private set; }

    private CrawlRun()
    {
    }

    private CrawlRun(Guid id, string sourceKey, VehicleCategory category, DateTime startedAt)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("SourceKey is required.", nameof(sourceKey));
        }

        Id = id;
        SourceKey = sourceKey;
        Category = category;
        StartedAt = startedAt;
        Status = CrawlStatus.Running;
    }

    public static CrawlRun Start(string sourceKey, VehicleCategory category, DateTime startedAt)
        => new(Guid.NewGuid(), sourceKey, category, startedAt);

    public void Complete(DateTime finishedAt, int itemsFound, int itemsSaved)
    {
        FinishedAt = finishedAt;
        ItemsFound = itemsFound;
        ItemsSaved = itemsSaved;
        Status = CrawlStatus.Completed;
    }

    public void Fail(DateTime finishedAt, string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Error message is required.", nameof(error));
        }

        FinishedAt = finishedAt;
        Error = error;
        Status = CrawlStatus.Failed;
    }
}
