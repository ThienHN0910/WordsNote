using Application.IRepositories.AS;
using Domain.Entities.AS;
using MongoDB.Driver;

namespace Infrastructure.Repositories.AS;

public class UserRepository : IUserRepo
{
    private const string UserCollectionName = "wordsnote_users";

    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>(UserCollectionName);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        User? user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user;
    }

    public async Task<User?> UpdateUserProfileAsync(Guid userId, User userUpdated)
    {
        var existing = await _users.Find(user => user.Id == userId).FirstOrDefaultAsync();
        if (existing == null)
        {
            return null;
        }

        existing.UserName = string.IsNullOrWhiteSpace(userUpdated.UserName) ? existing.UserName : userUpdated.UserName;
        existing.Name = userUpdated.Name;
        existing.Bio = userUpdated.Bio;
        existing.AvatarUrl = userUpdated.AvatarUrl;
        existing.UpdatedAt = DateTime.UtcNow;

        await _users.ReplaceOneAsync(user => user.Id == userId, existing);
        return existing;
    }
}
