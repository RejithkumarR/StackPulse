namespace StackPulse.Api.Services.Interfaces;

public interface ISecretConnectionStringResolver
{
    Task<(string? MySqlConnectionString, string? MongoConnectionString)> ResolveAsync(CancellationToken cancellationToken = default);
}
