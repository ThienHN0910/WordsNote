using WordsNote.Domain.Events;

namespace WordsNote.Domain.Entities;

public class Deck : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string UserId { get; private set; } = string.Empty;

    private readonly List<Card> _cards = new();
    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    private Deck() { }

    public Deck(string name, string description, string userId)
    {
        Name = name;
        Description = description;
        UserId = userId;

        AddDomainEvent(new DeckCreatedEvent(Id, name));
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void AddCard(Card card)
    {
        _cards.Add(card);
        AddDomainEvent(new CardAddedEvent(Id, card.Id));
    }

    public void RemoveCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card != null)
            _cards.Remove(card);
    }

    public void ResetProgress()
    {
        foreach (var card in _cards)
            card.ResetProgress();
    }
}
