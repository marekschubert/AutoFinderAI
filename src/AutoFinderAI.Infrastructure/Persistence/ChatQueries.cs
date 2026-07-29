using System.Text.Json;
using AutoFinderAI.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

public sealed class ChatQueries : IChatQueries
{
    private readonly AppDbContext _dbContext;
    private readonly IVehicleQueries _vehicleQueries;

    public ChatQueries(AppDbContext dbContext, IVehicleQueries vehicleQueries)
    {
        _dbContext = dbContext;
        _vehicleQueries = vehicleQueries;
    }

    public async Task<IReadOnlyList<ChatSessionSummaryDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChatSessions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastMessageAt)
            .Select(s => new ChatSessionSummaryDto(s.Id, s.Title, s.CreatedAt, s.LastMessageAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSessionDetailDto?> GetSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken)
    {
        var session = await _dbContext.ChatSessions.AsNoTracking()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Select(s => new { s.Id, s.Title, s.CreatedAt, s.LastMessageAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        var messages = await _dbContext.ChatMessages.AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto(
                m.Id, m.Role, m.Content, m.CriteriaJson, m.ResultVehicleIdsJson, m.ModelUsed, m.CreatedAt))
            .ToListAsync(cancellationToken);

        var allIds = messages
            .Where(m => !string.IsNullOrWhiteSpace(m.ResultVehicleIdsJson))
            .SelectMany(m => DeserializeIds(m.ResultVehicleIdsJson))
            .Distinct()
            .ToList();

        if (allIds.Count == 0)
        {
            return new ChatSessionDetailDto(session.Id, session.Title, session.CreatedAt, session.LastMessageAt, messages);
        }

        var vehicles = await _vehicleQueries.GetByIdsAsync(allIds, cancellationToken);
        var vehiclesById = vehicles.ToDictionary(v => v.Id);

        var messagesWithResults = messages
            .Select(m =>
            {
                if (string.IsNullOrWhiteSpace(m.ResultVehicleIdsJson))
                {
                    return m;
                }

                var results = DeserializeIds(m.ResultVehicleIdsJson)
                    .Select(id => vehiclesById.GetValueOrDefault(id))
                    .Where(v => v is not null)
                    .Select(v => v!)
                    .ToList();

                return results.Count == 0 ? m : m with { Results = results };
            })
            .ToList();

        return new ChatSessionDetailDto(session.Id, session.Title, session.CreatedAt, session.LastMessageAt, messagesWithResults);
    }

    private static List<Guid> DeserializeIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Guid>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch (JsonException)
        {
            return new List<Guid>();
        }
    }
}

