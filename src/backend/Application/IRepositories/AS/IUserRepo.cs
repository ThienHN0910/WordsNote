using Domain.Entities.AS;

namespace Application.IRepositories.AS
{
    public interface IUserRepo
    {
        Task<User?> UpdateUserProfileAsync(Guid userId, User userUpdated);
        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
