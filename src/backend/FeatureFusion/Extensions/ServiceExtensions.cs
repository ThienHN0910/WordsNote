using Application.Helpers;
using Application.IServices.AS;
using Application.Services.AS;
using Infrastructure.Repositories.AS;

namespace FeatureFusion.Extensions
{
    public static class ServiceExtensions
    {
        public static object AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Auth
            services.AddScoped<IAuthService, AuthService>();


            // Current User
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // User
            services.AddScoped<IUserService, UserService>();


            // Storage

            services.AddSingleton<JwtTokenGenerator>();
            return services;
        }
    }
}
