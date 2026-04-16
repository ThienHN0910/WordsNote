using Microsoft.AspNetCore.Mvc;
using Application.IServices.AS;
using Application.Dtos.AS;
using MongoDB.Driver;
using Google.Apis.Auth;
namespace FeatureFusion.Controllers.AS
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public IActionResult Register()
        {
            return StatusCode(StatusCodes.Status410Gone, new { Error = "Register is disabled. Please use Google login." });
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

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return BadRequest(new { Error = "Google ID token is required." });
            }

            var googleClientId = _configuration["AuthProviders:Google:ClientId"];
            var allowedEmail = _configuration["AuthProviders:Google:AdminEmail"];

            if (string.IsNullOrWhiteSpace(googleClientId))
            {
                _logger.LogError("Google login is not configured. Missing AuthProviders:Google:ClientId");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Google login is not configured." });
            }

            if (string.IsNullOrWhiteSpace(allowedEmail))
            {
                _logger.LogError("Google login is not configured. Missing AuthProviders:Google:AdminEmail");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Allowed admin email is not configured." });
            }

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [googleClientId],
                    });

                if (!payload.EmailVerified)
                {
                    return Unauthorized(new { Error = "Google email is not verified." });
                }

                var normalizedAllowedEmail = allowedEmail.Trim().ToLowerInvariant();
                var normalizedEmail = payload.Email.Trim().ToLowerInvariant();

                if (!string.Equals(normalizedEmail, normalizedAllowedEmail, StringComparison.Ordinal))
                {
                    return Unauthorized(new { Error = "This Google account is not allowed." });
                }

                var token = await _authService.LoginWithGoogleAsync(normalizedEmail, payload.Name);
                return Ok(token);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token received.");
                return Unauthorized(new { Error = "Invalid Google token." });
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Google login failed due to MongoDB error for email {Email}", "<masked>");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Authentication service is temporarily unavailable." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google login failed unexpectedly.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Unexpected authentication error." });
            }
        }
    }
}