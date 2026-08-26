namespace StackPulse.Api.Models;

public class RoleAccess
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;
    public Menu Menu { get; set; } = null!;
}
