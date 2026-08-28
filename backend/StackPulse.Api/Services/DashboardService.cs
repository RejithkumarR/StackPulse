using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
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
        var totalUsers = await _dbContext.users.CountAsync(cancellationToken);
        var activeUsers = await _dbContext.users.CountAsync(u => u.IsActive, cancellationToken);
        var auditLogFilter = Builders<Models.Mongo.MongoAuditLog>.Filter.Exists(x => x.Action, true);
        var totalAuditLogs = _mongoContext.IsConfigured
            ? await _mongoContext.AuditLogs.CountDocumentsAsync(auditLogFilter, cancellationToken: cancellationToken)
            : await _dbContext.AuditLogs.CountAsync(cancellationToken);
        var integrationStats = await GetIntegrationStatsAsync(cancellationToken);

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
            ,JiraProjectCount = integrationStats.JiraProjects.Count
            ,BitbucketRepositoryCount = integrationStats.BitbucketRepositories.Count
            ,JiraProjects = integrationStats.JiraProjects
            ,BitbucketRepositories = integrationStats.BitbucketRepositories
        };
    }

    private async Task<(IReadOnlyCollection<string> JiraProjects, IReadOnlyCollection<string> BitbucketRepositories)> GetIntegrationStatsAsync(CancellationToken cancellationToken)
    {
        if (!_mongoContext.IsConfigured)
        {
            var projects = await _dbContext.JiraIssues
                .Where(x => x.ProjectKey != null)
                .Select(x => x.ProjectKey!)
                .Distinct()
                .ToListAsync(cancellationToken);
            var repositoryNames = await _dbContext.BitbucketPullRequests
                .Where(x => x.Repo != null)
                .Select(x => x.Repo!)
                .Distinct()
                .ToListAsync(cancellationToken);
            return (projects, repositoryNames);
        }

        var jira = await GetLatestIntegrationPayloadAsync("Jira", cancellationToken);
        var bitbucket = await GetLatestIntegrationPayloadAsync("Bitbucket", cancellationToken);
        var jiraProjects = jira
            .Select(x => GetPayloadString(x, "projectKey") ?? GetPayloadString(x, "key")?.Split('-', 2)[0])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        var repositories = bitbucket
            .Select(x => GetPayloadString(x, "repo") ?? GetPayloadString(x, "repoSlug"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        return (jiraProjects, repositories);
    }

    private async Task<IReadOnlyCollection<BsonDocument>> GetLatestIntegrationPayloadAsync(string provider, CancellationToken cancellationToken)
    {
        var document = await _mongoContext.IntegrationSync
            .Find(Builders<BsonDocument>.Filter.Eq("provider", provider))
            .SortByDescending(x => x["completedAt"])
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null || !document.TryGetValue("payload", out var payload) || !payload.IsBsonArray)
        {
            return Array.Empty<BsonDocument>();
        }

        return payload.AsBsonArray.OfType<BsonDocument>().ToList();
    }

    private static string? GetPayloadString(BsonDocument document, string name) =>
        document.TryGetValue(name, out var value) && value.IsString ? value.AsString : null;

    public async Task<IReadOnlyCollection<ActivityItemDto>> GetRecentActivityAsync(CancellationToken cancellationToken = default)
    {
        if (_mongoContext.IsConfigured)
        {
            var mongoItems = await _mongoContext.AuditLogs
                .Find(Builders<Models.Mongo.MongoAuditLog>.Filter.Exists(x => x.Action, true))
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
