using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Action).HasColumnName("action");
        builder.Property(x => x.Details).HasColumnName("details");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Action).IsRequired().HasMaxLength(200);

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
