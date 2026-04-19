using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WordsNote.Desktop.Services;
using WordsNote.Desktop.Services.Configuration;
using WordsNote.Desktop.ViewModels;

namespace WordsNote.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private readonly IHost _host;

	public App()
	{
		_host = CreateHostBuilder().Build();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		await _host.StartAsync();
		var mainWindow = _host.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		await _host.StopAsync(TimeSpan.FromSeconds(5));
		_host.Dispose();

		base.OnExit(e);
	}

	private static IHostBuilder CreateHostBuilder()
	{
		var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

		return Host.CreateDefaultBuilder()
			.UseContentRoot(AppContext.BaseDirectory)
			.ConfigureAppConfiguration((_, config) =>
			{
				config.Sources.Clear();
				config.SetBasePath(AppContext.BaseDirectory);
				config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				config.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
				config.AddEnvironmentVariables(prefix: "WORDSNOTE_");
			})
			.ConfigureServices((context, services) =>
			{
				services.AddSingleton<IOptions<DesktopRuntimeOptions>>(_ =>
					Options.Create(CreateDesktopRuntimeOptions(context.Configuration)));

				services.AddSingleton<IAppDataPathProvider, AppDataPathProvider>();
				services.AddSingleton<GoogleBrowserAuthService>();
				services.AddSingleton<LocalManageStorageService>();
				services.AddSingleton<DesktopSettingsStorageService>();
				services.AddHttpClient<WordsNoteApiClient>();

				services.AddSingleton<MainViewModel>();
				services.AddSingleton<MainWindow>();
			});
	}

	private static DesktopRuntimeOptions CreateDesktopRuntimeOptions(IConfiguration configuration)
	{
		var section = configuration.GetSection(DesktopRuntimeOptions.SectionName);

		return new DesktopRuntimeOptions
		{
			ApiBaseUrl = section["ApiBaseUrl"] ?? "http://words-note.runasp.net",
			GoogleClientId = section["GoogleClientId"] ?? string.Empty,
			ThemeMode = section["ThemeMode"] ?? "light",
			LocalDataFolderName = section["LocalDataFolderName"] ?? "WordsNote\\desktop",
		};
	}
}
