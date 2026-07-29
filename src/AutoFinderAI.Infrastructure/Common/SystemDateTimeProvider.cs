using AutoFinderAI.Application.Abstractions;

namespace AutoFinderAI.Infrastructure.Common;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
