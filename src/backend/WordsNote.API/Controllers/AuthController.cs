using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WordsNote.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login()
    {
        return StatusCode(StatusCodes.Status410Gone, new
        {
            Message = "Local username/password login was removed. Use Supabase Auth (Google) on frontend and send Supabase access_token as Bearer token."
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue("email"),
            Role = User.FindFirstValue("role")
        });
    }
    
    [HttpGet("provider")]
    public IActionResult Provider()
    {
        var supabase = _configuration.GetSection("SupabaseAuth");
        var enabled = bool.TryParse(supabase["Enabled"], out var isEnabled) && isEnabled;

        return Ok(new
        {
            Enabled = enabled,
            Authority = supabase["Authority"],
            Audience = supabase["Audience"] ?? "authenticated",
            LoginProvider = enabled ? "supabase-google" : "local-jwt"
        });
    }
}
