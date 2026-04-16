using Microsoft.AspNetCore.Mvc;
using Application.IServices.AS;
using Application.Dtos.AS;
using MongoDB.Driver;
namespace FeatureFusion.Controllers.AS
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { Message = "User registered successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Register failed due to MongoDB error for userName {UserName} and email {Email}", request.UserName, request.Email);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Authentication service is temporarily unavailable." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed unexpectedly for userName {UserName} and email {Email}", request.UserName, request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Unexpected authentication error." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var user = await _authService.LoginAsync(request);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Login failed due to MongoDB error for userName {UserName} and email {Email}", request.UserName, request.Email);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Authentication service is temporarily unavailable." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed unexpectedly for userName {UserName} and email {Email}", request.UserName, request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Unexpected authentication error." });
            }
        }
    }
}