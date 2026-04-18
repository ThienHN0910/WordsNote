namespace WordsNote.Desktop.Models;

public sealed class StudyCard
{
    public string Id { get; set; } = string.Empty;

    public string CollectionId { get; set; } = string.Empty;

    public string? DeckId { get; set; }

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public List<string> Tags { get; set; } = [];

    public string DueAt { get; set; } = string.Empty;

    public string? LastReviewedAt { get; set; }

    public int Streak { get; set; }
}
