using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackPulse.Api.Models;

public class BitbucketPullRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Repo { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? State { get; set; }
    public string? Url { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("MachineInventory")]
    public Guid? MachineInventoryId { get; set; }
    public MachineInventory? MachineInventory { get; set; }
}
