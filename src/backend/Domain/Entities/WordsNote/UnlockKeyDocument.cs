using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.WordsNote;

public class UnlockKeyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("code")]
    public string Code { get; set; } = null!;

    [BsonElement("isUsed")]
    public bool IsUsed { get; set; } = false;

    [BsonElement("usedAt")]
    public DateTime? UsedAt { get; set; }

    [BsonElement("usedByEmail")]
    public string? UsedByEmail { get; set; }

    [BsonElement("targetSubjects")]
    public List<string> TargetSubjects { get; set; } = ["jfe301", "jit401"];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
