using System.IO;
using System.Text.Json;

namespace WordsNote.Desktop.Services;

public sealed class DesktopAppSettings
{
    public string ApiBaseUrl { get; set; } = "http://words-note.runasp.net";

    public string GoogleClientId { get; set; } = string.Empty;
}

public sealed class DesktopSettingsStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly string _filePath;

    public DesktopSettingsStorageService()
    {
        var baseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WordsNote",
            "desktop");
        Directory.CreateDirectory(baseFolder);
        _filePath = Path.Combine(baseFolder, "desktop-settings.json");
    }

    public async Task<DesktopAppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new DesktopAppSettings();
        }

        await using var stream = File.OpenRead(_filePath);
        var settings = await JsonSerializer.DeserializeAsync<DesktopAppSettings>(stream, JsonOptions, cancellationToken);
        if (settings is null || string.IsNullOrWhiteSpace(settings.ApiBaseUrl))
        {
            return new DesktopAppSettings();
        }

        settings.ApiBaseUrl = settings.ApiBaseUrl.Trim();
        settings.GoogleClientId = settings.GoogleClientId?.Trim() ?? string.Empty;
        return settings;
    }

    public async Task SaveAsync(DesktopAppSettings settings, CancellationToken cancellationToken = default)
    {
        var payload = new DesktopAppSettings
        {
            ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl)
                ? "http://words-note.runasp.net"
                : settings.ApiBaseUrl.Trim(),
            GoogleClientId = settings.GoogleClientId?.Trim() ?? string.Empty,
        };

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, payload, JsonOptions, cancellationToken);
    }
}
