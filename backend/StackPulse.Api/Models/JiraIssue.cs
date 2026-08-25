using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class JiraIssue
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Key { get; set; }
    public string? Summary { get; set; }
    public string? Status { get; set; }
    public string? Url { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("MachineInventory")]
    public Guid? MachineInventoryId { get; set; }
    public MachineInventory? MachineInventory { get; set; }
}
