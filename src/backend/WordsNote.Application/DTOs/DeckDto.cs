namespace WordsNote.Application.DTOs;

public class DeckDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CardCount { get; set; }
    public int DueCardCount { get; set; }
}
