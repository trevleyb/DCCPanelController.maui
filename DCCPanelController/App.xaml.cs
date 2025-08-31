using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace DCCPanelController;

public partial class App : Application {
    
    private ProfileService _profileService;
    private IServiceProvider _serviceProvider;

    public App(ProfileService profileService, IServiceProvider serviceProvider) {
        _profileService = profileService;
        _serviceProvider = serviceProvider;
        InitializeComponent();
        BindingDiagnostics.BindingFailed += BindingDiagnosticsOnBindingFailed;
    }

    /// <summary>
    ///     Gets the current <see cref="App" /> instance in use
    /// </summary>
    public new static App Current => Application.Current as App ?? throw new InvalidOperationException("Current application is not an instance of App");

    private void BindingDiagnosticsOnBindingFailed(object? sender, BindingBaseErrorEventArgs e) {
        var logger = LogHelper.CreateLogger("BindingDiagnostics");
        logger.LogWarning("Binding Failed: {BindingSource} | {BindingLine} | {BindingName} | {BindingMessage} | {BindingType}",
                          e?.XamlSourceInfo?.SourceUri.ToString() ?? "?SourceURI",
                          e?.XamlSourceInfo?.LineNumber.ToString() ?? "?LineNum",
                          e?.Binding?.ToString() ?? "?Binding",
                          e?.Message ?? "?Message",
                          e?.Binding?.GetType().Name ?? "?BindingType");
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        return new Window(new LoadingPage());
    }

    protected override async void OnStart() {
        base.OnStart();
        
        try {
            await _profileService.EnsureInitializedAsync(); // async, no blocking
            var shell = _serviceProvider.GetRequiredService<AppShell>(); // or new AppShell()
            var window = Windows.FirstOrDefault();
            if (window != null) {
                MainThread.BeginInvokeOnMainThread(() => window.Page = shell);
            }
        }
        catch (Exception ex) {
            throw new ApplicationException($"Failed to start app: {ex.Message}");
        }
        
#if DEBUG

        // 1) Ensure we extract/copy help to the app data folder
        await HelpService.Current.InitializeAsync(true);

        // 2) Validate the bundle (Resources/Raw) paths
        await ValidateBundleAsync();

        // 3) Validate the extracted files in InstalledRoot
        await ValidateExtractedAsync();
#endif
    }

    private static async Task ValidateBundleAsync() {
        // Manifest is in the *bundle*
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        Console.WriteLine("— Bundle validation —");
        foreach (var file in manifest.Files) {
            try {
                // NOTE: Use PackedRoot + filename for bundle access
                await using var s = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/{file}");
                Console.WriteLine($"✅ Bundled '{file}' length={s.Length}");
            } catch (Exception ex) {
                Console.WriteLine($"❌ Bundled '{file}' missing/inaccessible: {ex.Message}");
            }
        }
    }

    private static async Task ValidateExtractedAsync() {
        // Read manifest again (from bundle is fine)
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        Console.WriteLine("— Extracted validation —");
        foreach (var file in manifest.Files) {
            var p = Path.Combine(HelpService.InstalledRoot, file);
            if (File.Exists(p))
                Console.WriteLine($"✅ Extracted '{file}' length={new FileInfo(p).Length} at {p}");
            else
                Console.WriteLine($"❌ Extracted '{file}' missing at {p}");
        }
    }
}