using Application.Helpers;
using Application.IRepositories.AS;
using Application.IServices.AS;
using Application.Services.AS;
using Application.Storage;
using Infrastructure.Repositories.AS;
using Infrastructure.Services;

namespace FeatureFusion.Extensions
{
    public static class ServiceExtensions
    {
        public static object AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Auth
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // User
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepo, UserRepo>();

            // Storage
            services.AddScoped<IFileStorageService, SupabaseS3StorageService>();

            services.AddSingleton<JwtTokenGenerator>();
            return services;
        }
    }
}
