namespace WordsNote.Domain.Events;

public class DeckCreatedEvent : DomainEvent
{
    public Guid DeckId { get; }
    public string Name { get; }

    public DeckCreatedEvent(Guid deckId, string name)
    {
        DeckId = deckId;
        Name = name;
    }
}
