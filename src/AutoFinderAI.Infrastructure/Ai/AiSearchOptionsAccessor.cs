using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Infrastructure.Ai;

/// <summary>Exposes the Application-facing IAiSearchOptions view over the Infrastructure-owned
/// AiOptions, keeping Application decoupled from Infrastructure.Options.</summary>
public sealed class AiSearchOptionsAccessor : IAiSearchOptions
{
    private readonly IOptions<AiOptions> _options;

    public AiSearchOptionsAccessor(IOptions<AiOptions> options)
    {
        _options = options;
    }

    public int MaxCandidates => _options.Value.MaxCandidates;

    public int DefaultLimit => _options.Value.DefaultLimit;

    public int MaxLimit => _options.Value.MaxLimit;

    public int MaxRepairRetries => _options.Value.MaxRepairRetries;
}
