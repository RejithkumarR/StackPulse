using System.Text.Json;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;
using StackPulse.Api.Configuration;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Services;

public class AwsSecretsManagerConnectionStringResolver : ISecretConnectionStringResolver
{
    private readonly AwsSecretsManagerSettings _settings;

    public AwsSecretsManagerConnectionStringResolver(IOptions<AwsSecretsManagerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<(string? MySqlConnectionString, string? MongoConnectionString)> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return (null, null);
        }

        using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_settings.Region));
        return await ReadApplicationSecretAsync(client, _settings.SecretName, cancellationToken);
    }

    private static async Task<(string? MySqlConnectionString, string? MongoConnectionString)> ReadApplicationSecretAsync(
        IAmazonSecretsManager client,
        string? secretName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            return (null, null);
        }

        var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName }, cancellationToken);
        if (string.IsNullOrWhiteSpace(response.SecretString))
        {
            return (null, null);
        }

        using var document = JsonDocument.Parse(response.SecretString);
        var mysql = ReadString(document.RootElement, "mysqlConnectionString")
            ?? ReadString(document.RootElement, "MySqlConnectionString")
            ?? ReadString(document.RootElement, "MYSQL_CONNECTION_STRING");
        var mongo = ReadString(document.RootElement, "mongoConnectionString")
            ?? ReadString(document.RootElement, "MongoConnectionString")
            ?? ReadString(document.RootElement, "MONGO_CONNECTION_STRING");

        return (mysql, mongo);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }
}
