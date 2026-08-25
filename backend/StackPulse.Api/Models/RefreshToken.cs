using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
