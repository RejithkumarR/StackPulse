namespace StackPulse.Api.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "stackpulse_transactions";
    public string AuditCollection { get; set; } = "audit_logs";
    public string ApplicationLogCollection { get; set; } = "application_logs";
    public string MachineInventoryCollection { get; set; } = "machine_inventory";
    public string IntegrationSyncCollection { get; set; } = "integration_sync";
}
