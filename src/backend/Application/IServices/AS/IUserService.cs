using Domain.Entities.AS;

namespace Application.IServices.AS
{
    public interface IUserService
    {
        Task<User?> UpdateUserProfileAsync(Guid userId, User userUpdated);
        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
