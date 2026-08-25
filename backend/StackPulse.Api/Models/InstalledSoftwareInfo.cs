using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class InstalledSoftwareInfo
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Publisher { get; set; }

    [ForeignKey("MachineInventory")]
    public Guid MachineInventoryId { get; set; }
    public MachineInventory? MachineInventory { get; set; }
}
