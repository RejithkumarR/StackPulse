using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class ComputerMasterConfiguration : IEntityTypeConfiguration<ComputerMaster>
{
    public void Configure(EntityTypeBuilder<ComputerMaster> builder)
    {
        builder.ToTable("computer_masters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Hostname).HasColumnName("hostname");
        builder.Property(x => x.AssetTag).HasColumnName("asset_tag");
        builder.Property(x => x.Owner).HasColumnName("owner");
        builder.Property(x => x.Environment).HasColumnName("environment");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.Hostname).IsRequired().HasMaxLength(120);
        builder.Property(x => x.AssetTag).HasMaxLength(80);
        builder.Property(x => x.Owner).HasMaxLength(120);
        builder.Property(x => x.Environment).HasMaxLength(80);
        builder.HasIndex(x => x.Hostname).IsUnique();
    }
}
