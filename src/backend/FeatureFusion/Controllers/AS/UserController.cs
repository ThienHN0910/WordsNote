using Application.IServices.AS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities.AS;
using Application.Facades;
namespace FeatureFusion.Controllers.AS;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly UserFacade _userFacade;
    public UserController(ICurrentUserService currentUserService, IUserService userService, UserFacade userFacade)
    {
        _currentUserService = currentUserService;
        _userService = userService;
        _userFacade = userFacade;
    }
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized();
        }
        var user = await _userService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] User userUpdated)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized();
        }
        var updatedUser = await _userFacade.UpdateUserProfileAsync(userId.Value, userUpdated);
        if (updatedUser == null)
        {
            return NotFound();
        }
        return Ok(updatedUser);
    }
}
