using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class Role
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RoleAccess> RoleAccesses { get; set; } = new List<RoleAccess>();
}
