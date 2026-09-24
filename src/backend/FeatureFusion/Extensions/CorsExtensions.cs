
namespace FeatureFusion.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var configuredOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var normalizedOrigins = configuredOrigins
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim().TrimEnd('/'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Always ensure production domains and fallbacks are included
            normalizedOrigins.Add("https://words-note.thienhn.io.vn");
            normalizedOrigins.Add("http://words-note.thienhn.io.vn");
            normalizedOrigins.Add("https://words-note-five.vercel.app");
            normalizedOrigins.Add("http://words-note.runasp.net");
            normalizedOrigins.Add("https://words-note.runasp.net");

            services.AddCors(options =>
            {
                options.AddPolicy("AllowVue", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin)) return false;
                        var clean = origin.Trim().TrimEnd('/');
                        if (normalizedOrigins.Contains(clean)) return true;

                        if (Uri.TryCreate(clean, UriKind.Absolute, out var uri))
                        {
                            // Allow localhost and local loopback on any port for development
                            if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            // Allow *.thienhn.io.vn subdomains
                            if (uri.Host.Equals("thienhn.io.vn", StringComparison.OrdinalIgnoreCase) ||
                                uri.Host.EndsWith(".thienhn.io.vn", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }

                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            return services;
        }
    }
}
