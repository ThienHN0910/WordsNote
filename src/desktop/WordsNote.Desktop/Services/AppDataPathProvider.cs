using System.IO;
using WordsNote.Desktop.Services.Configuration;
using Microsoft.Extensions.Options;

namespace WordsNote.Desktop.Services;

public interface IAppDataPathProvider
{
    string GetFilePath(string fileName);
}

public sealed class AppDataPathProvider : IAppDataPathProvider
{
    private readonly string _baseDirectory;

    public AppDataPathProvider(IOptions<DesktopRuntimeOptions> options)
    {
        var configuredFolder = options.Value.LocalDataFolderName?.Trim();
        if (string.IsNullOrWhiteSpace(configuredFolder))
        {
            configuredFolder = "WordsNote\\desktop";
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var normalizedFolder = configuredFolder
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);

        _baseDirectory = Path.Combine(localAppData, normalizedFolder);
        Directory.CreateDirectory(_baseDirectory);
    }

    public string GetFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        return Path.Combine(_baseDirectory, fileName);
    }
}
