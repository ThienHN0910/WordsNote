using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WordsNote.Desktop.Services.Configuration;
using WordsNote.Desktop.Services.Serialization;

namespace WordsNote.Desktop.Services;

public sealed class DesktopAppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;

    public string GoogleClientId { get; set; } = string.Empty;

    public string AuthToken { get; set; } = string.Empty;

    public string ThemeMode { get; set; } = "light";
}

public sealed class DesktopSettingsStorageService
{
    private readonly string _filePath;
    private readonly DesktopRuntimeOptions _defaults;

    public DesktopSettingsStorageService(IAppDataPathProvider pathProvider, IOptions<DesktopRuntimeOptions> runtimeOptions)
    {
        _filePath = pathProvider.GetFilePath("desktop-settings.json");
        _defaults = runtimeOptions.Value;
    }

    public async Task<DesktopAppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefaultSettings();
        }

        await using var stream = File.OpenRead(_filePath);
        var settings = await JsonSerializer.DeserializeAsync(
            stream,
            WordsNoteJsonSerializerContext.Default.DesktopAppSettings,
            cancellationToken);
        if (settings is null)
        {
            return CreateDefaultSettings();
        }

        settings.ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl)
            ? _defaults.ApiBaseUrl.Trim()
            : settings.ApiBaseUrl.Trim();
        settings.GoogleClientId = settings.GoogleClientId?.Trim() ?? string.Empty;
        settings.AuthToken = settings.AuthToken?.Trim() ?? string.Empty;
        settings.ThemeMode = NormalizeThemeMode(settings.ThemeMode);
        return settings;
    }

    public async Task SaveAsync(DesktopAppSettings settings, CancellationToken cancellationToken = default)
    {
        var payload = new DesktopAppSettings
        {
            ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl)
                ? _defaults.ApiBaseUrl.Trim()
                : settings.ApiBaseUrl.Trim(),
            GoogleClientId = settings.GoogleClientId?.Trim() ?? string.Empty,
            AuthToken = settings.AuthToken?.Trim() ?? string.Empty,
            ThemeMode = NormalizeThemeMode(settings.ThemeMode),
        };

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(
            stream,
            payload,
            WordsNoteJsonSerializerContext.Default.DesktopAppSettings,
            cancellationToken);
    }

    private static string NormalizeThemeMode(string? value)
    {
        return string.Equals(value?.Trim(), "dark", StringComparison.OrdinalIgnoreCase)
            ? "dark"
            : "light";
    }

    private DesktopAppSettings CreateDefaultSettings()
    {
        return new DesktopAppSettings
        {
            ApiBaseUrl = _defaults.ApiBaseUrl.Trim(),
            GoogleClientId = _defaults.GoogleClientId?.Trim() ?? string.Empty,
            AuthToken = string.Empty,
            ThemeMode = NormalizeThemeMode(_defaults.ThemeMode),
        };
    }
}
