using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.TileSelectors;
#if IOS
using AVFoundation;
#endif

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage, INotifyPropertyChanged {
    private       bool   _started;
    private       Task?  _startupTask;
    private const double Gap = 12;

    private readonly SemaphoreSlim _once = new(1, 1);

    private string _statusText = "Starting…";
    private double _progress;

    public string StatusText {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    public double Progress {
        get => _progress;
        private set => Set(ref _progress, value);
    }

    public LoadingPage() {
        InitializeComponent();
        BindingContext = this;

        VersionLabel.Text = $"v{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

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
        _ = _startupTask.ContinueWith(t => { Debug.WriteLine(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
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

            #if IOS
            await StepAsync("Configuring audio…", 0.06, async () => {
                try {
                    var session = AVAudioSession.SharedInstance();
                    session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers, out _);
                    session.SetMode(AVAudioSessionMode.Default, out _);
                    session.SetActive(true, out _);
                } catch (Exception ex) {
                    Debug.WriteLine($"Audio init warning: {ex.Message}");
                }
                await Task.CompletedTask;
            });
            #endif

            await StepAsync("Loading profile…", 0.18, () => profileService.InitializeAsync());

            await StepAsync("Initialising connection service…", 0.30, () => connectionService.InitializeAsync());

            await StepAsync("Preparing help system…", 0.40, () => HelpService.Current.InitializeAsync(true));

            await StepAsync("Validating bundled content…", 0.52, ValidateBundleAsync);

            await StepAsync("Checking extracted content…", 0.64, ValidateExtractedAsync);

            await StepAsync("Building palette cache…", 0.78, async () => {
                TileSelectorPaletteCache.PrebuildDefaultPalette();
                await Task.CompletedTask;
            });

            await StepAsync("Playing startup sound…", 0.86, async () => {
                if (profileService.ActiveProfile?.Settings?.PlayStartupSound == true) {}
                    // Removed this as it was annoying
                    //await ClickSounds.PlayStartupSoundAsync();
            });

            await StepAsync("Starting UI…", 0.94, async () => {
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
            await MainThread.InvokeOnMainThreadAsync(async () => { await DisplayAlert("Startup error", ex.Message, "OK"); });
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

    private static async Task ValidateBundleAsync() {
        await using var ms = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/manifest.json");
        using var reader = new StreamReader(ms);
        var manifestJson = await reader.ReadToEndAsync();
        var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson)!;

        foreach (var file in manifest.Files) {
            try {
                await using var _ = await FileSystem.OpenAppPackageFileAsync($"{HelpService.PackedRoot}/{file}");
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