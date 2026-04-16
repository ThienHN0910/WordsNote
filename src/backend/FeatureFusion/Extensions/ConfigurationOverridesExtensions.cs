namespace FeatureFusion.Extensions;

public static class ConfigurationOverridesExtensions
{
    public static void ApplyFlatEnvironmentOverrides(this IConfigurationManager configuration)
    {
        var env = Environment.GetEnvironmentVariables();
        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        AddIfPresent(env, overrides, "MONGODB_URI", "MongoDB:ConnectionString");
        AddIfPresent(env, overrides, "MONGODB_DATABASE_NAME", "MongoDB:DatabaseName");
        AddIfPresent(env, overrides, "JWT_SECRET", "JwtSettings:SecretKey");
        AddIfPresent(env, overrides, "GOOGLE_CLIENT_ID", "AuthProviders:Google:ClientId");
        AddIfPresent(env, overrides, "GOOGLE_CLIENT_SECRET", "AuthProviders:Google:ClientSecret");
        AddIfPresent(env, overrides, "ADMIN_EMAIL", "AuthProviders:Google:AdminEmail");
        AddIfPresent(env, overrides, "API_BASE_URL", "ApiBaseUrl");
        AddIfPresent(env, overrides, "FRONTEND_URL", "FrontendUrl");

        if (TryGetString(env, "API_BASE_URL", out var apiBaseUrl))
        {
            overrides["JwtSettings:Issuer"] = apiBaseUrl;
        }

        if (TryGetString(env, "FRONTEND_URL", out var frontendUrl))
        {
            overrides["JwtSettings:Audience"] = frontendUrl;
        }

        if (TryGetString(env, "CORS_ORIGIN", out var corsOriginsRaw))
        {
            var origins = corsOriginsRaw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            for (var index = 0; index < origins.Length; index += 1)
            {
                overrides[$"AllowedOrigins:{index}"] = origins[index];
            }
        }

        if (overrides.Count > 0)
        {
            configuration.AddInMemoryCollection(overrides!);
        }
    }

    private static void AddIfPresent(System.Collections.IDictionary env, IDictionary<string, string?> target, string envKey, string configKey)
    {
        if (TryGetString(env, envKey, out var value))
        {
            target[configKey] = value;
        }
    }

    private static bool TryGetString(System.Collections.IDictionary env, string key, out string value)
    {
        var raw = env[key]?.ToString();
        if (!string.IsNullOrWhiteSpace(raw))
        {
            value = raw;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
