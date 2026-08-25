using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class DriveInfoEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Name { get; set; }
    public long? TotalBytes { get; set; }
    public long? FreeBytes { get; set; }
    public string? DriveType { get; set; }

    [ForeignKey("MachineInventory")]
    public Guid MachineInventoryId { get; set; }
    public MachineInventory? MachineInventory { get; set; }
}
