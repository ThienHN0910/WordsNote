namespace Application.Dtos.WordsNote;

public class StudyCardDTO
{
    public string Id { get; set; } = string.Empty;

    public string DeckId { get; set; } = string.Empty;

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public List<string> Tags { get; set; } = [];

    public string DueAt { get; set; } = string.Empty;

    public string? LastReviewedAt { get; set; }

    public int Streak { get; set; }
}