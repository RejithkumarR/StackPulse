using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using StackPulse.Api.Data;
using StackPulse.Api.DTOs.Dashboard;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly StackPulseDbContext _dbContext;
    private readonly MongoStackPulseContext _mongoContext;

    public DashboardService(StackPulseDbContext dbContext, MongoStackPulseContext mongoContext)
    {
        _dbContext = dbContext;
        _mongoContext = mongoContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _dbContext.Users.CountAsync(cancellationToken);
        var activeUsers = await _dbContext.Users.CountAsync(u => u.IsActive, cancellationToken);
        var totalAuditLogs = _mongoContext.IsConfigured
            ? await _mongoContext.AuditLogs.CountDocumentsAsync(FilterDefinition<Models.Mongo.MongoAuditLog>.Empty, cancellationToken: cancellationToken)
            : await _dbContext.AuditLogs.CountAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            SystemHealth = "Healthy",
            Uptime = "99.98%",
            Alerts = 2,
            ActiveSessions = 18,
            LastUpdated = DateTime.UtcNow,
            TotalAuditLogs = (int)Math.Min(totalAuditLogs, int.MaxValue)
        };
    }

    public async Task<IReadOnlyCollection<ActivityItemDto>> GetRecentActivityAsync(CancellationToken cancellationToken = default)
    {
        if (_mongoContext.IsConfigured)
        {
            var mongoItems = await _mongoContext.AuditLogs
                .Find(FilterDefinition<Models.Mongo.MongoAuditLog>.Empty)
                .SortByDescending(x => x.CreatedAt)
                .Limit(5)
                .ToListAsync(cancellationToken);

            return mongoItems.Select(x => new ActivityItemDto
            {
                Id = Guid.TryParse(x.MasterEntityId, out var id) ? id : x.MasterUserId,
                Action = x.Action,
                Details = x.Details ?? "No additional details",
                CreatedAt = x.CreatedAt,
                UserName = x.UserName
            }).ToList();
        }

        var items = await _dbContext.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new ActivityItemDto
            {
                Id = x.Id,
                Action = x.Action,
                Details = x.Details ?? "No additional details",
                CreatedAt = x.CreatedAt,
                UserName = x.User.Username
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
