using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data;

public class StackPulseDbContext : DbContext
{
    public StackPulseDbContext(DbContextOptions<StackPulseDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StackPulseDbContext).Assembly);
    }
}
