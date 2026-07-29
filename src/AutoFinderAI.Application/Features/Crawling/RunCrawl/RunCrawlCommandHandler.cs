using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Common;
using AutoFinderAI.Application.Features.Crawling.Mapping;
using AutoFinderAI.Domain.Crawling;
using AutoFinderAI.Domain.Vehicles;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoFinderAI.Application.Features.Crawling.RunCrawl;

/// <summary>
/// Runs a full crawl for one category: creates a <see cref="CrawlRun"/>, streams raw listings from
/// the matching <see cref="IListingSourceAdapter"/>, upserts each into the <see cref="IVehicleRepository"/>.
/// A single invalid listing is logged and skipped — it never aborts the run.
/// </summary>
public sealed class RunCrawlCommandHandler : IRequestHandler<RunCrawlCommand, Result<RunCrawlResult>>
{
    private static readonly Error NoAdapter =
        Error.NotFound("Crawl.NoAdapter", "No listing source adapter supports this category.");

    private readonly IEnumerable<IListingSourceAdapter> _adapters;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICrawlRunRepository _crawlRunRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RunCrawlCommandHandler> _logger;

    public RunCrawlCommandHandler(
        IEnumerable<IListingSourceAdapter> adapters,
        IVehicleRepository vehicleRepository,
        ICrawlRunRepository crawlRunRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        ILogger<RunCrawlCommandHandler> logger)
    {
        _adapters = adapters;
        _vehicleRepository = vehicleRepository;
        _crawlRunRepository = crawlRunRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RunCrawlResult>> Handle(RunCrawlCommand request, CancellationToken cancellationToken)
    {
        var adapter = _adapters.FirstOrDefault(a => a.Supported.Contains(request.Category));
        if (adapter is null)
        {
            return NoAdapter;
        }

        var startedAt = _dateTimeProvider.UtcNow;
        var crawlRun = CrawlRun.Start(adapter.SourceKey, request.Category, startedAt);
        await _crawlRunRepository.AddAsync(crawlRun, cancellationToken);

        var itemsFound = 0;
        var itemsSaved = 0;
        // Guards against the same (SourceKey, ExternalId) being yielded twice within one run
        // (e.g. overlapping list pages) before it has actually been persisted.
        var processedInThisRun = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            await foreach (var raw in adapter.CrawlAsync(request.Category, cancellationToken))
            {
                itemsFound++;

                if (!processedInThisRun.Add(raw.ExternalId))
                {
                    _logger.LogWarning(
                        "Skipped listing {ExternalId} from {SourceKey}: already processed in this crawl run.",
                        raw.ExternalId, adapter.SourceKey);
                    continue;
                }

                Vehicle? saved = null;

                try
                {
                    var scrapedAt = _dateTimeProvider.UtcNow;
                    var existing = await _vehicleRepository.FindBySourceAsync(adapter.SourceKey, raw.ExternalId, cancellationToken);

                    if (existing is Car existingCar)
                    {
                        RawListingToCarMapper.ApplyUpdate(existingCar, raw, adapter.SourceKey, scrapedAt);
                        saved = existingCar;
                    }
                    else if (RawListingToCarMapper.TryMap(raw, adapter.SourceKey, scrapedAt, out var car, out var mapError))
                    {
                        await _vehicleRepository.AddAsync(car!, cancellationToken);
                        saved = car;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Skipped listing {ExternalId} from {SourceKey}: {Reason}",
                            raw.ExternalId, adapter.SourceKey, mapError);
                        continue;
                    }

                    _logger.LogInformation(
                        "Parsed vehicle {@Vehicle} for {ExternalId} from {SourceKey}.",
                        saved, raw.ExternalId, adapter.SourceKey);

                    // Saved per item, not bulked at the end: isolates a bad row (duplicate,
                    // constraint violation) from the rest of a long-running crawl.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    itemsSaved++;
                }
                catch (Exception itemEx)
                {
                    // Covers unexpected mapping errors and database save conflicts (e.g. a
                    // duplicate ExternalId already present from a previous run). Detaching the
                    // rejected entity keeps the change tracker clean for the next iteration.
                    _logger.LogWarning(itemEx, "Skipped listing {ExternalId} from {SourceKey} due to a save error.", raw.ExternalId, adapter.SourceKey);
                    if (saved is not null)
                    {
                        _vehicleRepository.Detach(saved);
                    }
                }
            }

            crawlRun.Complete(_dateTimeProvider.UtcNow, itemsFound, itemsSaved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crawl run {CrawlRunId} for {SourceKey} failed.", crawlRun.Id, adapter.SourceKey);
            crawlRun.Fail(_dateTimeProvider.UtcNow, ex.Message);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist final status of crawl run {CrawlRunId}.", crawlRun.Id);
        }

        return new RunCrawlResult(crawlRun.Id, crawlRun.Status, itemsFound, itemsSaved, crawlRun.Error);
    }
}
