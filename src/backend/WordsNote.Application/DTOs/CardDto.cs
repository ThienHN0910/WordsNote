using WordsNote.Domain.Enums;

namespace WordsNote.Application.DTOs;

public class CardDto
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public CardStatus Status { get; set; }
    public int Interval { get; set; }
    public DateTime NextReviewDate { get; set; }
}
