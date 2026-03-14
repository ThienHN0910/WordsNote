using WordsNote.Domain.Entities;

namespace WordsNote.Domain.Interfaces;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id);
    Task<IEnumerable<Card>> GetByDeckIdAsync(Guid deckId);
    Task<Dictionary<Guid, int>> GetCardCountsByDeckIdsAsync(IEnumerable<Guid> deckIds);
    Task<Dictionary<Guid, int>> GetDueCardCountsByDeckIdsAsync(IEnumerable<Guid> deckIds, DateTime date);
    Task<IEnumerable<Card>> GetDueCardsAsync(string userId, DateTime date);
    Task AddAsync(Card card);
    Task UpdateAsync(Card card);
    Task DeleteAsync(Guid id);
    Task AddRangeAsync(IEnumerable<Card> cards);
    Task UpdateRangeAsync(IEnumerable<Card> cards);
}
