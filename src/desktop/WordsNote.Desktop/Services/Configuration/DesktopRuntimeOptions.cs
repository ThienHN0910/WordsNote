namespace WordsNote.Desktop.Services.Configuration;

public sealed class DesktopRuntimeOptions
{
    public const string SectionName = "Desktop";

    public string ApiBaseUrl { get; set; } = "http://words-note.runasp.net";

    public string GoogleClientId { get; set; } = string.Empty;

    public string ThemeMode { get; set; } = "light";

    public string LocalDataFolderName { get; set; } = "WordsNote\\desktop";
}
