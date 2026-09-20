using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.WordsNote;

public class QuizSetDocument
{
    [BsonId]
    public string Id { get; set; } = default!;

    public string Code { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = "#2563eb";

    public int TotalQuestions { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
