using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StackPulse.Api.Models.Mongo;

public class ApplicationLogEntry
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? TraceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
