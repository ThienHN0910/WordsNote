namespace Application.Dtos.AS
{
    public class LoginRequestDTO
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
    }
}