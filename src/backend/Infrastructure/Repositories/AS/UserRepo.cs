using Application.IRepositories.AS;
using Domain.Entities.AS;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories.AS
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;
        public UserRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User> UpdateUserProfileAsync(Guid userId, User userUpdated)
        {
            var userToUpdate = await _context.Users.FindAsync(userId);
            if (userUpdated.Name != null)
            {
                userToUpdate.Name = userUpdated.Name;
            }
            if (userUpdated.Bio != null)
            {
                userToUpdate.Bio = userUpdated.Bio;
            }
            if (userUpdated.AvatarUrl != null)
            {
                userToUpdate.AvatarUrl = userUpdated.AvatarUrl;
            }
            userToUpdate.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return userToUpdate;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user;
        }
    }
}
