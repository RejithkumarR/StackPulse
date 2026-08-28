using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class GitHubPullRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Repository { get; set; }
    public int Number { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? State { get; set; }
    public string? Url { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}