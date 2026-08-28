namespace StackPulse.Api.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public string SystemHealth { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public int Alerts { get; set; }
    public int ActiveSessions { get; set; }
    public DateTime LastUpdated { get; set; }
    public int TotalAuditLogs { get; set; }
    public int JiraProjectCount { get; set; }
    public int BitbucketRepositoryCount { get; set; }
    public IReadOnlyCollection<string> JiraProjects { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> BitbucketRepositories { get; set; } = Array.Empty<string>();
}

public class ActivityItemDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
}
