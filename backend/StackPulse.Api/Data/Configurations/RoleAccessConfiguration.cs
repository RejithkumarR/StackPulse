using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class RoleAccessConfiguration : IEntityTypeConfiguration<RoleAccess>
{
    public void Configure(EntityTypeBuilder<RoleAccess> builder)
    {
        builder.ToTable("role_accesses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RoleId).HasColumnName("role_id");
        builder.Property(x => x.MenuId).HasColumnName("menu_id");
        builder.Property(x => x.CanView).HasColumnName("can_view");
        builder.Property(x => x.CanCreate).HasColumnName("can_create");
        builder.Property(x => x.CanUpdate).HasColumnName("can_update");
        builder.Property(x => x.CanDelete).HasColumnName("can_delete");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RoleAccesses)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Menu)
            .WithMany(x => x.RoleAccesses)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
