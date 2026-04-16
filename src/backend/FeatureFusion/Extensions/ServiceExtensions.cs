using Application.Helpers;
using Application.IRepositories.AS;
using Application.IServices.AS;
using Application.Facades;
using Application.Services.AS;
using Infrastructure.Repositories.AS;

namespace FeatureFusion.Extensions
{
    public static class ServiceExtensions
    {
        public static object AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepo, UserRepository>();

            // Auth
            services.AddScoped<IAuthService, AuthService>();


            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // User
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<UserFacade>();


            // Storage

            services.AddSingleton<JwtTokenGenerator>();
            return services;
        }
    }
}
