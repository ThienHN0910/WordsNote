using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.WordsNote;

public class DeskDocument
{
    [BsonId]
    public string Id { get; set; } = default!;

    public Guid UserId { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}