namespace StackPulse.Api.DTOs.MasterConfiguration;

public class MasterConfigurationDto
{
    public IReadOnlyCollection<ComputerMasterDto> Computers { get; set; } = Array.Empty<ComputerMasterDto>();
    public IReadOnlyCollection<IntegrationAccessDto> Integrations { get; set; } = Array.Empty<IntegrationAccessDto>();
}
