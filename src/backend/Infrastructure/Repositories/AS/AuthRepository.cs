using Domain.Entities.AS;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.IRepositories.AS;

namespace Infrastructure.Repositories.AS;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> CheckUserExistsAsync(string UserName, string Email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName || u.Email == Email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username, string passwordhash)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.PasswordHash == passwordhash);
    }

    public async Task<User?> GetUserByEmailAsync(string email, string passwordhash)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordhash);
    }


}