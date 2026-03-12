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

        return Ok(new
        {
            Enabled = true,
            Authority = supabase["Authority"],
            Audience = supabase["Audience"] ?? "authenticated",
            LoginProvider = "supabase-google"
        });
    }
}
