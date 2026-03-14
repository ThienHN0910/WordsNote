using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.AS;
namespace Application.IRepositories.AS
{
    public interface IAuthRepository
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username, string passwordhash);
        Task<User?> GetUserByEmailAsync(string email, string passwordhash);
        Task<User?> CheckUserExistsAsync(string UserName, string Email);
    }
}