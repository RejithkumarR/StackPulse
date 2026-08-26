using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class ComputerMaster
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Hostname { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? AssetTag { get; set; }

    [MaxLength(120)]
    public string? Owner { get; set; }

    [MaxLength(80)]
    public string? Environment { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
