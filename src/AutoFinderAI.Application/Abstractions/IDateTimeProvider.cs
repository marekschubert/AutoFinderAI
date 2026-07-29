namespace AutoFinderAI.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
