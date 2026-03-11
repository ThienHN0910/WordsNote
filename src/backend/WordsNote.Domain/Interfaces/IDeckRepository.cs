using WordsNote.Domain.Entities;

namespace WordsNote.Domain.Interfaces;

public interface IDeckRepository
{
    Task<Deck?> GetByIdAsync(Guid id);
    Task<IEnumerable<Deck>> GetByUserIdAsync(string userId);
    Task AddAsync(Deck deck);
    Task UpdateAsync(Deck deck);
    Task DeleteAsync(Guid id);
}
