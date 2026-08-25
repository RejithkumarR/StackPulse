using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class MachineInventory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Hostname { get; set; }
    public string? OSVersion { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    public List<WindowsServiceInfo>? WindowsServices { get; set; }
    public List<InstalledSoftwareInfo>? InstalledSoftwares { get; set; }
    public List<DriveInfoEntry>? Drives { get; set; }
}
