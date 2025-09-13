using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage {
    private bool _started;

    public LoadingPage() {
        InitializeComponent();
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

                try {
                    // 1) Initialize domain state
                    await profileService.InitializeAsync();
                    
                    
                    // Optional: any other one-time startup work here (help assets, migrations, etc.)
                    await HelpService.Current.InitializeAsync(true);
#if DEBUG
                    await ValidateBundleAsync();
                    await ValidateExtractedAsync();
#endif
                    // 2) Only now create Shell and replace the root
                    var shell = services.GetRequiredService<AppShell>();

                    // Important: swap the root page instead of navigating
                    var window = Application.Current?.Windows[0];
                    window?.Page = shell; // Avoid Application.Current.MainPage (obsolete)
                } catch (Exception ex) {
                    await DisplayAlert("Startup error", ex.Message,"Quit");
                    App.Current.Quit();
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
            if (!File.Exists(p))
                Debug.WriteLine($"❌ Extracted '{file}' missing at {p}");
        }
    }
}