using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class WindowsServiceInfo
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? ServiceName { get; set; }
    public string? DisplayName { get; set; }
    public string? State { get; set; }
    public string? StartMode { get; set; }

    [ForeignKey("MachineInventory")]
    public Guid MachineInventoryId { get; set; }
    public MachineInventory? MachineInventory { get; set; }
}
