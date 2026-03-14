using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.WordsNote;

public class CardDocument
{
    [BsonId]
    public string Id { get; set; } = default!;

    public string DeskId { get; set; } = default!;

    public Guid UserId { get; set; }

    public string Front { get; set; } = default!;

    public string Back { get; set; } = default!;

    public string? Hint { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime DueAt { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public int Streak { get; set; }
}