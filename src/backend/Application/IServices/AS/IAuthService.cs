using Application.Dtos.AS;

namespace Application.IServices.AS
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestDTO request);

        Task<string> LoginAsync(LoginRequestDTO request);


        Task<string> LoginByUsernameAsync(string username, string password);

        Task<string> LoginByEmailAsync(string email, string password);

        Task<string> LoginWithGoogleAsync(string email, string? displayName = null);
    }
}
