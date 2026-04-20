using Application.Helpers;
using Application.IRepositories.AS;
using Application.IRepositories.FFP;
using Application.IServices.AS;
using Application.IServices.FFP;
using Application.Facades;
using Application.Services.AS;
using Application.Services.FFP;
using Infrastructure.Repositories.AS;
using Infrastructure.Repositories.FFP;

namespace FeatureFusion.Extensions
{
    public static class ServiceExtensions
    {
        public static object AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepo, UserRepository>();
            services.AddScoped<IDownloadPageConfigRepository, DownloadPageConfigRepository>();

            // Auth
            services.AddScoped<IAuthService, AuthService>();


            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // User
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<UserFacade>();

            // FFP
            services.AddScoped<IDownloadPageConfigService, DownloadPageConfigService>();


            // Storage

            services.AddSingleton<JwtTokenGenerator>();
            return services;
        }
    }
}
