using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class IntegrationAccessConfiguration : IEntityTypeConfiguration<IntegrationAccess>
{
    public void Configure(EntityTypeBuilder<IntegrationAccess> builder)
    {
        builder.ToTable("integration_accesses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Provider).HasColumnName("provider");
        builder.Property(x => x.DisplayName).HasColumnName("display_name");
        builder.Property(x => x.BaseUrl).HasColumnName("base_url");
        builder.Property(x => x.ProjectKey).HasColumnName("project_key");
        builder.Property(x => x.Username).HasColumnName("username");
        builder.Property(x => x.SecretReference).HasColumnName("secret_reference");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.Provider).IsRequired().HasMaxLength(40);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(160);
        builder.Property(x => x.BaseUrl).IsRequired().HasMaxLength(300);
        builder.Property(x => x.ProjectKey).HasMaxLength(160);
        builder.Property(x => x.Username).HasMaxLength(200);
        builder.Property(x => x.SecretReference).HasMaxLength(500);
        builder.HasIndex(x => new { x.Provider, x.DisplayName }).IsUnique();
    }
}
