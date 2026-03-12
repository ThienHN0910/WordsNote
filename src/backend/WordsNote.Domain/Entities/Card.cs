using WordsNote.Domain.Enums;

namespace WordsNote.Domain.Entities;

public class Card : BaseEntity
{
    public Guid DeckId { get; private set; }
    public string Front { get; private set; } = string.Empty;
    public string Back { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public CardStatus Status { get; private set; } = CardStatus.New;
    public int Interval { get; private set; } = 1;
    public double EaseFactor { get; private set; } = 2.5;
    public DateTime NextReviewDate { get; private set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Card() { }

    public Card(Guid deckId, string front, string back, string notes = "")
    {
        DeckId = deckId;
        Front = front;
        Back = back;
        Notes = notes;
    }

    public void Update(string front, string back, string notes)
    {
        Front = front;
        Back = back;
        Notes = notes;
    }

    public void UpdateReview(bool wasCorrect)
    {
        if (wasCorrect)
        {
            Interval = Math.Max(1, (int)Math.Round(Interval * EaseFactor));
            Status = Interval < 21 ? CardStatus.Learning : CardStatus.Learned;
        }
        else
        {
            Interval = 1;
            EaseFactor = Math.Max(1.3, EaseFactor - 0.2);
            Status = CardStatus.New;
        }

        NextReviewDate = DateTime.UtcNow.AddDays(Interval);
    }

    public void ResetProgress()
    {
        Status = CardStatus.New;
        Interval = 1;
        EaseFactor = 2.5;
        NextReviewDate = DateTime.UtcNow;
    }
}
