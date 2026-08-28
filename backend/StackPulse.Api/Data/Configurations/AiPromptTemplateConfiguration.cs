using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackPulse.Api.Models;

namespace StackPulse.Api.Data.Configurations;

public class AiPromptTemplateConfiguration : IEntityTypeConfiguration<AiPromptTemplate>
{
    public void Configure(EntityTypeBuilder<AiPromptTemplate> builder)
    {
        builder.ToTable("ai_prompt_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Key).HasColumnName("prompt_key").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(160);
        builder.Property(x => x.Template).HasColumnName("template").IsRequired().HasColumnType("LONGTEXT");
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.Key, x.Version }).IsUnique();
    }
}