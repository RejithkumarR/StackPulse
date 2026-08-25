using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
