using MongoDB.Driver;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;
using WordsNote.Infrastructure.Persistence;

namespace WordsNote.Infrastructure.Repositories;

public class DeckRepository : IDeckRepository
{
    private readonly MongoDbContext _context;

    public DeckRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Deck?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Deck>.Filter.Eq(d => d.Id, id);
        return await _context.Decks.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Deck>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<Deck>.Filter.Eq(d => d.UserId, userId);
        return await _context.Decks.Find(filter).ToListAsync();
    }

    public async Task AddAsync(Deck deck)
    {
        await _context.Decks.InsertOneAsync(deck);
    }

    public async Task UpdateAsync(Deck deck)
    {
        var filter = Builders<Deck>.Filter.Eq(d => d.Id, deck.Id);
        await _context.Decks.ReplaceOneAsync(filter, deck);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Deck>.Filter.Eq(d => d.Id, id);
        await _context.Decks.DeleteOneAsync(filter);
    }
}
