namespace StackPulse.Api.Configuration;

public class DatabaseSettings
{
    public string Provider { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
}
