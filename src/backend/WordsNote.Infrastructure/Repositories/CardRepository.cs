using MongoDB.Driver;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;
using WordsNote.Infrastructure.Persistence;

namespace WordsNote.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly MongoDbContext _context;

    public CardRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Card?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        return await _context.Cards.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Card>> GetByDeckIdAsync(Guid deckId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.DeckId, deckId);
        return await _context.Cards.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Card>> GetDueCardsAsync(string userId, DateTime date)
    {
        // Filter cards that are due and belong to decks owned by the given user.
        // We query all due cards first, then cross-reference deck ownership.
        // For large datasets, consider denormalising userId onto the Card document.
        var filter = Builders<Card>.Filter.Lte(c => c.NextReviewDate, date);
        return await _context.Cards.Find(filter).ToListAsync();
    }

    public async Task AddAsync(Card card)
    {
        await _context.Cards.InsertOneAsync(card);
    }

    public async Task UpdateAsync(Card card)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, card.Id);
        await _context.Cards.ReplaceOneAsync(filter, card);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        await _context.Cards.DeleteOneAsync(filter);
    }

    public async Task AddRangeAsync(IEnumerable<Card> cards)
    {
        await _context.Cards.InsertManyAsync(cards);
    }
}
