using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;
using StackPulse.Api.DTOs.Dashboard;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly StackPulseDbContext _dbContext;

    public DashboardService(StackPulseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _dbContext.Users.CountAsync(cancellationToken);
        var activeUsers = await _dbContext.Users.CountAsync(u => u.IsActive, cancellationToken);
        var totalAuditLogs = await _dbContext.AuditLogs.CountAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            SystemHealth = "Healthy",
            Uptime = "99.98%",
            Alerts = 2,
            ActiveSessions = 18,
            LastUpdated = DateTime.UtcNow,
            TotalAuditLogs = totalAuditLogs
        };
    }

    public async Task<IReadOnlyCollection<ActivityItemDto>> GetRecentActivityAsync(CancellationToken cancellationToken = default)
    {
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
