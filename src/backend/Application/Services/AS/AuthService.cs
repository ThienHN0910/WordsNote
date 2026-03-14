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
        if (await _authRepository.CheckUserExistsAsync(request.UserName, request.Email) != null)
        {
            throw new ArgumentException("User already exists.");
        }
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = HashPassSHA256.HashPass(request.Password)
        };

        await _authRepository.AddUserAsync(user);

    }
    public async Task<string> LoginAsync(LoginRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
           return await LoginByEmailAsync(request.Email, request.Password);
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return await LoginByUsernameAsync(request.UserName, request.Password);
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


}
