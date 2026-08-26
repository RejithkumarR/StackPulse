namespace StackPulse.Api.DTOs.MasterConfiguration;

public class IntegrationAccessDto
{
    public Guid? Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ProjectKey { get; set; }
    public string? Username { get; set; }
    public string? SecretReference { get; set; }
    public bool IsActive { get; set; } = true;
}
