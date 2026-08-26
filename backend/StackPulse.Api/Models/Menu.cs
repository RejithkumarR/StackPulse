using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class Menu
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Path { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoleAccess> RoleAccesses { get; set; } = new List<RoleAccess>();
}
