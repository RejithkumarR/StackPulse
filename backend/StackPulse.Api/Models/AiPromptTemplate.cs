using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class AiPromptTemplate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}