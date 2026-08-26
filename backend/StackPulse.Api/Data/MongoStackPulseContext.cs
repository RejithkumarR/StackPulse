using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StackPulse.Api.Configuration;
using StackPulse.Api.Models.Mongo;

namespace StackPulse.Api.Data;

public class MongoStackPulseContext
{
    private readonly IMongoDatabase? _database;
    private readonly MongoDbSettings _settings;

    public MongoStackPulseContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        if (!string.IsNullOrWhiteSpace(_settings.ConnectionString))
        {
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }
    }

    public bool IsConfigured => _database is not null;

    public IMongoCollection<MongoAuditLog> AuditLogs =>
        GetCollection<MongoAuditLog>(_settings.AuditCollection);

    public IMongoCollection<ApplicationLogEntry> ApplicationLogs =>
        GetCollection<ApplicationLogEntry>(_settings.ApplicationLogCollection);

    public IMongoCollection<BsonDocument> MachineInventory =>
        GetCollection<BsonDocument>(_settings.MachineInventoryCollection);

    public IMongoCollection<BsonDocument> IntegrationSync =>
        GetCollection<BsonDocument>(_settings.IntegrationSyncCollection);

    private IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        if (_database is null)
        {
            throw new InvalidOperationException("MongoDB connection is not configured.");
        }

        return _database.GetCollection<T>(collectionName);
    }
}
