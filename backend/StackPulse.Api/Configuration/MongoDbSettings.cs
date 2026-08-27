namespace StackPulse.Api.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "stackpulse";
    public string AuditCollection { get; set; } = "audit_logs";
    public string ApplicationLogCollection { get; set; } = "stackpulse_logs";
    public string MachineInventoryCollection { get; set; } = "transactions";
    public string IntegrationSyncCollection { get; set; } = "transactions";
}
