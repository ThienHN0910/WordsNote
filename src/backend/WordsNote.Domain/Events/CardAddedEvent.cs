namespace WordsNote.Domain.Events;

public class CardAddedEvent : DomainEvent
{
    public Guid DeckId { get; }
    public Guid CardId { get; }

    public CardAddedEvent(Guid deckId, Guid cardId)
    {
        DeckId = deckId;
        CardId = cardId;
    }
}
