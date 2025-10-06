using System.Diagnostics;
using System.Text.Json;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.TileSelectors;
using AVFoundation;
using DCCPanelController.Models.ViewModel.Helpers;

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage {
    private bool   _started;
    const   double Gap = 12;

    public LoadingPage() {
        InitializeComponent();
        BindingContext = this;
        VersionLabel.Text = $"v{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

        SplashLogo.SizeChanged += (_, __) => UpdateVersionOffset();
        this.SizeChanged += (_, __) => UpdateVersionOffset();
        
        NavigationPage.SetHasNavigationBar(this, false);
    }
    
    void UpdateVersionOffset() {
        if (SplashLogo.Height <= 0) return;
        VersionLabel.TranslationY = (SplashLogo.Height / 2.0) + Gap;
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

                    //System.Diagnostics.Debugger.Break();

                    // Initialize domain state
                    await profileService.InitializeAsync();
                    await connectionService.InitializeAsync();
                    
                    // Setup the Audio
                    #if IOS
                    try {
                        var session = AVAudioSession.SharedInstance();
                        session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out var catErr);
                        session.SetMode(AVAudioSessionMode.Default, out var modeErr);
                        session.SetActive(true, out var actErr);
                    } catch (Exception ex) {
                        Debug.WriteLine($"Error setting up audio: {ex.Message}");
                    }
                    #endif
                    
                    await ClickSounds.PlayStartupSoundAsync();
                    
                    // Any other one-time startup work here (help assets, migrations, etc.)
                    await HelpService.Current.InitializeAsync(true);

                    await ValidateBundleAsync();
                    await ValidateExtractedAsync();

                    // Only now create Shell and replace the root
                    var shell = services.GetRequiredService<AppShell>();

                    // Prebuild the Palette Cache
                    TileSelectorPaletteCache.PrebuildDefaultPalette();

                    await Task.Delay(1500); // Lets just make it look like it is doing something interesting....
                    
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