using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data;

public class StackPulseDbContext : DbContext
{
    public StackPulseDbContext(DbContextOptions<StackPulseDbContext> options) : base(options)
    {
    }

    public DbSet<User> users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RoleAccess> RoleAccesses => Set<RoleAccess>();
    public DbSet<ComputerMaster> ComputerMasters => Set<ComputerMaster>();
    public DbSet<IntegrationAccess> IntegrationAccesses => Set<IntegrationAccess>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MachineInventory> MachineInventories => Set<MachineInventory>();
    public DbSet<WindowsServiceInfo> WindowsServices => Set<WindowsServiceInfo>();
    public DbSet<InstalledSoftwareInfo> InstalledSoftwares => Set<InstalledSoftwareInfo>();
    public DbSet<DriveInfoEntry> Drives => Set<DriveInfoEntry>();
    public DbSet<JiraIssue> JiraIssues => Set<JiraIssue>();
    public DbSet<BitbucketPullRequest> BitbucketPullRequests => Set<BitbucketPullRequest>();
    public DbSet<GitHubPullRequest> GitHubPullRequests => Set<GitHubPullRequest>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StackPulseDbContext).Assembly);
    }
}
