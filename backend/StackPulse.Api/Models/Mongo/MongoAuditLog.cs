using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StackPulse.Api.Models.Mongo;

public class MongoAuditLog
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid MasterUserId { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? EntityName { get; set; }
    public string? MasterEntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
