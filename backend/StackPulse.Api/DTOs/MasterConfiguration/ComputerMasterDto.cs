namespace StackPulse.Api.DTOs.MasterConfiguration;

public class ComputerMasterDto
{
    public Guid? Id { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public string? Owner { get; set; }
    public string? Environment { get; set; }
    public bool IsActive { get; set; } = true;
}
