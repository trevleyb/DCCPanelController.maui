using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
#if IOS
using AVFoundation;
#endif

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage, INotifyPropertyChanged {
    private       bool   _started;
    private       Task?  _startupTask;
    private const double Gap = 12;

    private readonly SemaphoreSlim _once = new(1, 1);

    private string _statusText = "Starting";
    private double _progress;

    public string StatusText {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    public double Progress {
        get => _progress;
        private set => Set(ref _progress, value);
    }

    public new bool IsBusy { get; private set; }

    public LoadingPage() {
        BindingContext = this;
        InitializeComponent();

        VersionLabel.Text = $"v{AppInfo.Current.VersionString}";

        SplashLogo.SizeChanged += (_, __) => UpdateVersionOffset();
        SizeChanged += (_, __) => UpdateVersionOffset();

        NavigationPage.SetHasNavigationBar(this, false);
    }

    void UpdateVersionOffset() {
        if (SplashLogo.Height <= 0) return;
        VersionGrid.TranslationY = (SplashLogo.Height / 2.0) + Gap;
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (_started) return;
        _started = true;

        _startupTask = InitializeOnceAsync();
        _ = _startupTask.ContinueWith(t => { LogHelper.Logger.LogError(t.Exception,"DynamicTileProperty Form : OnAppearing Error"); }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task InitializeOnceAsync() {
        await _once.WaitAsync();
        try {
            IsBusy = true;
            await Task.Yield(); // let the page fully render

            var services = IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("No service provider.");

            var profileService = services.GetRequiredService<ProfileService>();
            var connectionService = services.GetRequiredService<ConnectionService>();

            ArgumentNullException.ThrowIfNull(profileService, nameof(profileService));
            ArgumentNullException.ThrowIfNull(connectionService, nameof(connectionService));

            // Perf logging config (before anything heavy)
            MethodTimeLogger.LogDirectory = Path.Combine(FileSystem.AppDataDirectory, "PerfLogs");
            MethodTimeLogger.MaxBytesBeforeRotate = 20 * 1024 * 1024;

            double step = 0.0;
            double steps = 11.0;
            
            #if IOS
            steps++;
            await StepAsync("Configuring audio…", ++step/steps, async () => {
                try {
                    var session = AVAudioSession.SharedInstance();
                    session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out _);
                    session.SetMode(AVAudioSessionMode.Default, out _);
                    session.SetActive(true, out _);
                } catch (Exception ex) {
                    LogHelper.Logger.LogWarning(ex, $"Audio init warning.");
                }
                await Task.CompletedTask;
            });
            #endif
            
            // 1)
            await StepAsync("Loading profile", ++step/steps, () => profileService.InitializeAsync());

            // 2)
            await StepAsync("Validating catalog", ++step/steps, () => profileService.ValidateCatalog());
            
            // 2.5)
            var mauiContext = Handler?.MauiContext;
            await StepAsync("Validating installed fonts", ++step/steps, await ValidateFontsAsync(mauiContext));
            
            // 3)
            await StepAsync("Initialising connection service", ++step/steps, () => connectionService.InitializeAsync());

            // 4)
            await StepAsync("Preparing help system",++step/steps, () => HelpService.Current.InitializeAsync(true));

            // 5)
            await StepAsync("Validating bundled content", ++step/steps, ValidateBundleAsync);

            // 6)
            await StepAsync("Checking extracted content", ++step/steps, ValidateExtractedAsync);

            // 7)
            await StepAsync("Building palette cache", ++step/steps, async () => {
                PaletteCache.PrebuildPalette("Side");
                PaletteCache.PrebuildPalette("Bottom");
                await Task.CompletedTask;
            });

            // 8)
            await StepAsync("Playing startup sound", ++step/steps, async () => {
                if (profileService.ActiveProfile?.Settings?.PlayStartupSound == true) {
                    await ClickSounds.PlayStartupSoundAsync();
                }
            });

            // 9)
            await StepAsync("Starting UI", ++step/steps, async () => {
                await MainThread.InvokeOnMainThreadAsync(() => {
                    var shell = services.GetRequiredService<AppShell>();
                    var window = Application.Current?.Windows[0];
                    if (window is not null) window.Page = shell;
                });
            });

            // cosmetic finish
            await AnimateToCompleteAsync();
            IsBusy = false;
        } catch (Exception ex) {
            await MainThread.InvokeOnMainThreadAsync(async () => { await DisplayAlertAsync("Startup error", ex.Message, "OK"); });
            throw;
        } finally {
            _once.Release();
            IsBusy = false;
        }
    }

    /// <summary>
    /// Runs an initialization step, updating status + progress safely on the UI thread.
    /// </summary>
    private async Task StepAsync(string message, double progressAfter, Func<Task> step) {
        if (progressAfter > 1.00) progressAfter = 1.00;
        await SetStatusAsync(message, progressAfter);
        await step();
    }

    private async Task SetStatusAsync(string message, double targetProgress) {
        await MainThread.InvokeOnMainThreadAsync(async () => {
            StatusText = message;
            var clamped = Math.Clamp(targetProgress, 0.0, 1.0);
            await this.ProgressTo(clamped, 220u, Easing.CubicOut);
            Progress = clamped; // keep the bound value in sync
        });
    }

    private async Task AnimateToCompleteAsync() {
        await MainThread.InvokeOnMainThreadAsync(async () => {
            await this.ProgressTo(1.0, 180u, Easing.CubicOut);
            Progress = 1.0;
            StatusText = "Ready";
        });
    }

    // Bindable ProgressBar animation helper (animate the Progress property of the page)
    private Task ProgressTo(double newValue, uint length, Easing easing) => this.AnimateAsync("LoadingProgressAnim", Progress, newValue, v => Progress = v, length, easing);

    // Small generic property animator
    private Task AnimateAsync(string name, double from, double to, Action<double> setter, uint length, Easing easing) {
        var tcs = new TaskCompletionSource();
        this.AbortAnimation(name);
        var anim = new Animation(v => setter(v), from, to, easing);
        anim.Commit(this, name, 16, length, finished: (_, __) => tcs.TrySetResult());
        return tcs.Task;
    }

    private void Set<T>(ref T backing, T value, [CallerMemberName] string? name = null) {
        if (EqualityComparer<T>.Default.Equals(backing, value)) return;
        backing = value;
        OnPropertyChanged(name);
    }

    private static async Task<Func<Task>> ValidateFontsAsync(IMauiContext? mauiContext) {
        if (mauiContext is null) return () => Task.CompletedTask;
        _ = FontValidationService.ValidateAsync(FontCatalog.RegisteredFonts, mauiContext).ContinueWith(t => {
            foreach (var r in t.Result) {
                if (!r.FileExists || !r.PlatformResolved) {
                    LogHelper.Logger.LogInformation($"❌[FONT] {r.Font.Alias} INVALID: {r.Detail}");
                }
            }
        });
        return () => Task.CompletedTask;
    }

    private static async Task ValidateBundleAsync() {
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        foreach (var file in manifest.Files) {
            try {
                await using var _ = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/{file}");
            } catch (Exception ex) {
                LogHelper.Logger.LogError(ex, $"❌ Bundled '{file}' missing/inaccessible: {ex.Message}");
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
                LogHelper.Logger.LogWarning($"❌ Extracted '{file}' missing at {p}");
        }
    }
}