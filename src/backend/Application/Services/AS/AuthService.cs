namespace Application.Services.AS;

using System.Threading.Tasks;
using Application.Helpers;
using Application.IRepositories.AS;
using Domain.Entities.AS;
using Application.IServices.AS;
using Application.Dtos.AS;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IAuthRepository authRepository, JwtTokenGenerator jwtTokenGenerator)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task RegisterAsync(RegisterRequestDTO request)
    {

        if (string.IsNullOrWhiteSpace(request.UserName) && string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Either UserName or Email must be provided.");
        }

        var userName = request.UserName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;

        if (await _authRepository.CheckUserExistsAsync(userName, email) != null)
        {
            throw new ArgumentException("User already exists.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        var user = new User
        {
            UserName = userName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            PasswordHash = HashPassSHA256.HashPass(request.Password)
        };

        await _authRepository.AddUserAsync(user);

    }
    public async Task<string> LoginAsync(LoginRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        var userName = request.UserName?.Trim();
        var email = request.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(email))
        {
            return await LoginByUsernameAsync(userName, request.Password);
        }

        if (!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userName))
        {
            return await LoginByEmailAsync(email, request.Password);
        }

        throw new ArgumentException("Either UserName or Email must be provided.");     
    }

    public async Task<string> LoginByUsernameAsync(string username, string password)
    {
        var user = await _authRepository.GetUserByUsernameAsync(username, HashPassSHA256.HashPass(password));
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }
        var token = _jwtTokenGenerator.GenerateToken(user);
        return token;
    }
    public async Task<string> LoginByEmailAsync(string email, string password)
    {
        var user = await _authRepository.GetUserByEmailAsync(email, HashPassSHA256.HashPass(password));
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        var token = _jwtTokenGenerator.GenerateToken(user);
        return token;
    }

    public async Task<string> LoginWithGoogleAsync(string email, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingUser = await _authRepository.CheckUserExistsAsync(string.Empty, normalizedEmail);

        if (existingUser == null)
        {
            var baseUserName = normalizedEmail.Split('@')[0];
            var resolvedUserName = await GenerateAvailableUserNameAsync(baseUserName);

            existingUser = new User
            {
                UserName = resolvedUserName,
                Email = normalizedEmail,
                Name = string.IsNullOrWhiteSpace(displayName) ? resolvedUserName : displayName.Trim(),
                PasswordHash = HashPassSHA256.HashPass($"google:{Guid.NewGuid():N}"),
            };

            await _authRepository.AddUserAsync(existingUser);
        }

        return _jwtTokenGenerator.GenerateToken(existingUser);
    }

    private async Task<string> GenerateAvailableUserNameAsync(string requestedUserName)
    {
        var sanitizedBase = string.IsNullOrWhiteSpace(requestedUserName)
            ? "user"
            : requestedUserName.Trim().ToLowerInvariant();

        var candidate = sanitizedBase;
        var suffix = 1;

        while (await _authRepository.CheckUserExistsAsync(candidate, string.Empty) != null)
        {
            candidate = $"{sanitizedBase}{suffix}";
            suffix += 1;
        }

        return candidate;
    }


}
