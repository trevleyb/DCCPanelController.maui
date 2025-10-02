using System.Diagnostics;
using System.Text.Json;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.TileSelectors;

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage {
    private bool _started;

    public LoadingPage() {
        InitializeComponent();
        BindingContext = this;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (_started) return;
        _started = true;

        // Run after the current appearance transition completes
        Dispatcher.Dispatch(async () => {
            if (IPlatformApplication.Current?.Services is { } services) {
                var profileService = services.GetRequiredService<ProfileService>();
                var connectionService = services.GetRequiredService<ConnectionService>();
                
                try {
                    DCCPanelController.Helpers.MethodTimeLogger.LogDirectory = Path.Combine(FileSystem.AppDataDirectory, "PerfLogs");
                    DCCPanelController.Helpers.MethodTimeLogger.MaxBytesBeforeRotate = 20 * 1024 * 1024; // 20 MB

                    // Initialize domain state
                    await profileService.InitializeAsync();
                    await connectionService.InitializeAsync();
                    
                    // Any other one-time startup work here (help assets, migrations, etc.)
                    await HelpService.Current.InitializeAsync(true);

                    #if DEBUG
                    await ValidateBundleAsync();
                    await ValidateExtractedAsync();
                    #endif

                    // Only now create Shell and replace the root
                    var shell = services.GetRequiredService<AppShell>();

                    // Prebuild the Palette Cache
                    TileSelectorPaletteCache.PrebuildDefaultPalette();

                    // Important: swap the root page instead of navigating
                    var window = Application.Current?.Windows[0];
                    window?.Page = shell; // Avoid Application.Current.MainPage (obsolete)
                } catch (Exception ex) {
                    await DisplayAlert("Startup error", ex.Message, "Quit");
                }
            }
        });
    }

    private static async Task ValidateBundleAsync() {
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        foreach (var file in manifest.Files) {
            try {
                await using var s = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/{file}");
            } catch (Exception ex) {
                Debug.WriteLine($"❌ Bundled '{file}' missing/inaccessible: {ex.Message}");
            }
        }
    }

    private static async Task ValidateExtractedAsync() {
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        foreach (var file in manifest.Files) {
            var p = Path.Combine(HelpService.InstalledRoot, file);
            if (!File.Exists(p)) {
                Debug.WriteLine($"❌ Extracted '{file}' missing at {p}");
            }
        }
    }
}