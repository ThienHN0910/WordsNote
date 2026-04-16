using Application.IRepositories.AS;
using Application.IServices.AS;
using Domain.Entities.AS;

namespace Application.Services.AS
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        public UserService(IUserRepo userRepo) {
           _userRepo = userRepo;
        }
        public Task<User?> GetUserByIdAsync(Guid userId)
        {
            return _userRepo.GetUserByIdAsync(userId);
        }

        public Task<User?> UpdateUserProfileAsync(Guid userId, User userUpdated)
        {
            return _userRepo.UpdateUserProfileAsync(userId, userUpdated);
        }
    }
}
