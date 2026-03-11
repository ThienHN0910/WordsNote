namespace WordsNote.Application.DTOs;

public class CreateCardDto
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Guid DeckId { get; set; }
}
