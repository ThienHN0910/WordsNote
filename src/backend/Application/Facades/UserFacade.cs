using Application.IServices.AS;
using Domain.Entities.AS;

namespace Application.Facades;

public class UserFacade
{
    private readonly IUserService _userService;
    public UserFacade(IUserService userService)
    {
        _userService = userService;
    }

    public Task<User?> UpdateUserProfileAsync(Guid userId, User userUpdated)
    {
        return _userService.UpdateUserProfileAsync(userId, userUpdated);
    }
}
