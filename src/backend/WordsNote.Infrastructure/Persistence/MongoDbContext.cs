using MongoDB.Driver;
using WordsNote.Domain.Entities;

namespace WordsNote.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Deck> Decks => _database.GetCollection<Deck>("Decks");
    public IMongoCollection<Card> Cards => _database.GetCollection<Card>("Cards");
}
