using Application.Dtos.FFP;
using Application.IServices.AS;
using Application.IServices.FFP;
using Domain.Entities.AS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFusion.Controllers.FFP;

[ApiController]
[Route("api/download-config")]
public class DownloadConfigController : ControllerBase
{
    private readonly IDownloadPageConfigService _downloadPageConfigService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly string _configuredAdminEmail;

    public DownloadConfigController(
        IDownloadPageConfigService downloadPageConfigService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IConfiguration configuration)
    {
        _downloadPageConfigService = downloadPageConfigService;
        _currentUserService = currentUserService;
        _userService = userService;
        _configuredAdminEmail = (configuration["AuthProviders:Google:AdminEmail"] ?? string.Empty).Trim().ToLowerInvariant();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await _downloadPageConfigService.GetAsync();
        return Ok(config);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] DownloadPageConfigDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId.Value);
        if (!IsAdminUser(user))
        {
            return Forbid();
        }

        var updated = await _downloadPageConfigService.UpsertAsync(request, user?.Email);
        return Ok(updated);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Reset()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId.Value);
        if (!IsAdminUser(user))
        {
            return Forbid();
        }

        await _downloadPageConfigService.ResetAsync();
        return NoContent();
    }

    private bool IsAdminUser(User? user)
    {
        if (user == null)
        {
            return false;
        }

        var role = (user.Role ?? string.Empty).Trim().ToLowerInvariant();
        if (role == "admin")
        {
            return true;
        }

        var email = (user.Email ?? string.Empty).Trim().ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(_configuredAdminEmail)
            && string.Equals(email, _configuredAdminEmail, StringComparison.Ordinal);
    }
}