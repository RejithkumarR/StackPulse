using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
