using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class ComputerMasterConfiguration : IEntityTypeConfiguration<ComputerMaster>
{
    public void Configure(EntityTypeBuilder<ComputerMaster> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Hostname).IsRequired().HasMaxLength(120);
        builder.Property(x => x.AssetTag).HasMaxLength(80);
        builder.Property(x => x.Owner).HasMaxLength(120);
        builder.Property(x => x.Environment).HasMaxLength(80);
        builder.HasIndex(x => x.Hostname).IsUnique();
    }
}
