using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class IntegrationAccess
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string Provider { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? ProjectKey { get; set; }

    [MaxLength(200)]
    public string? Username { get; set; }

    [MaxLength(500)]
    public string? SecretReference { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
