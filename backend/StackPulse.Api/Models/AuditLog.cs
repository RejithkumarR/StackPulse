using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
