namespace StackPulse.Api.Configuration;

public class AwsSecretsManagerSettings
{
    public bool Enabled { get; set; }
    public string Region { get; set; } = "us-east-1";
    public string? SecretName { get; set; }
}
