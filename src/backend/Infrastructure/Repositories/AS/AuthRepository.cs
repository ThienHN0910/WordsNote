using Application.IRepositories.AS;
using Domain.Entities.AS;
using MongoDB.Driver;

namespace Infrastructure.Repositories.AS;

public class AuthRepository : IAuthRepository
{
    private const string UserCollectionName = "wordsnote_users";

    private readonly IMongoCollection<User> _users;

    public AuthRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>(UserCollectionName);
    }

    public async Task AddUserAsync(User user)
    {
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }

        var now = DateTime.UtcNow;
        user.CreatedAt ??= now;
        user.UpdatedAt = now;
        user.IsActive ??= true;
        user.Role ??= "User";

        await _users.InsertOneAsync(user);
    }

    public async Task<User?> GetUserByUsernameAsync(string username, string passwordhash)
    {
        User? user = await _users
            .Find(user => user.UserName == username && user.PasswordHash == passwordhash)
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email, string passwordhash)
    {
        User? user = await _users
            .Find(user => user.Email == email && user.PasswordHash == passwordhash)
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<User?> CheckUserExistsAsync(string userName, string email)
    {
        if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var filters = new List<FilterDefinition<User>>();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            filters.Add(Builders<User>.Filter.Eq(user => user.UserName, userName));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            filters.Add(Builders<User>.Filter.Eq(user => user.Email, email));
        }

        var filter = filters.Count == 1
            ? filters[0]
            : Builders<User>.Filter.Or(filters);

        User? user = await _users.Find(filter).FirstOrDefaultAsync();
        return user;
    }
}
