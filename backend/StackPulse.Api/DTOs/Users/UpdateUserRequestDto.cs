using System.ComponentModel.DataAnnotations;

namespace StackPulse.Api.DTOs.Users;

public class UpdateUserRequestDto
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public bool IsActive { get; set; }
}
