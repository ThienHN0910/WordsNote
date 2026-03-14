using Application.IRepositories.CMS;
using Domain.Entities.CMS;
using MongoDB.Driver;

namespace Infrastructure.Repositories.CMS;
public class PostRepo : IPostRepo
{
    private const string CollectionName = "Posts";
    private readonly IMongoCollection<Post> _posts;

    public PostRepo(IMongoDatabase database)
    {
        _posts = database.GetCollection<Post>(CollectionName);
    }

    public async Task<Post> CreateAsync(Post entity)
    {
        await _posts.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(string entityId)
    {
        var result = await _posts.DeleteOneAsync(p => p.PostId == entityId);
        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<Post>> GetAllAsync(int page, int limit)
    {
        return await _posts.Find(_ => true)
                           .Skip((page - 1) * limit)
                           .Limit(limit)
                           .ToListAsync();
    }

    public async Task<Post> GetByIdAsync(string entityId)
    {
        return await _posts.Find(p => p.PostId == entityId).FirstOrDefaultAsync();
    }

    public async Task<Post> UpdateAsync(string entityId, Post entityUpdated)
    {
        entityUpdated.PostId = entityId;
        entityUpdated.UpdateAt = DateTime.UtcNow;
        var result = await _posts.ReplaceOneAsync(p => p.PostId == entityId, entityUpdated);
        return result.IsAcknowledged && result.ModifiedCount > 0 ? entityUpdated : null;
    }
}


