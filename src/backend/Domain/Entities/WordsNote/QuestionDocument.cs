using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.WordsNote;

public class QuestionDocument
{
    [BsonId]
    public string Id { get; set; } = default!;

    public string SubjectId { get; set; } = default!;

    public int QuestionNumber { get; set; }

    public string Question { get; set; } = default!;

    public Dictionary<string, string> Options { get; set; } = new();

    public List<string> Answers { get; set; } = new();

    public int Choose { get; set; } = 1;

    public string? Explanation { get; set; }

    public string? Note { get; set; }

    public string? Source { get; set; }

    public string? Exam { get; set; }
}
