using System.Collections.Concurrent;
using Plugin.Maui.Audio;
using AudioManager = Plugin.Maui.Audio.AudioManager;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class ClickSounds {
    // Hold both the player AND the backing stream so it never gets GC’d/disposed.
    private sealed class PlayerHolder : IDisposable {
        public IAudioPlayer Player { get; }
        public Stream Stream { get; } // kept alive for the lifetime of the player

        public PlayerHolder(IAudioPlayer player, Stream stream) {
            Player = player;
            Stream = stream;
        }

        public void Dispose() {
            Player?.Dispose();
            Stream?.Dispose();
        }
    }

    private static readonly IAudioManager                              Audio = AudioManager.Current;
    private static readonly ConcurrentDictionary<string, PlayerHolder> Cache = new();

    // Call this once at app startup to reduce first-play hiccups
    public static async Task PreloadAsync(params string[] filenames) {
        foreach (var f in filenames.Distinct())
            _ = await GetOrCreateAsync(f).ConfigureAwait(false);
    }

    public static Task PlayStartupSoundAsync() => PlayAsync("startupSound.wav");
    public static Task PlayError2SoundAsync() => PlayAsync("beep1.m4a");
    public static Task PlayError1SoundAsync() => PlayAsync("beep2.m4a");
    public static Task PlayRouteClickSoundAsync() => PlayAsync("Button_Click_Quick.m4a");
    public static Task PlaySwitchClickSoundAsync() => PlayAsync("Button_Light_Switch2.wav");
    public static Task PlayButtonClickSoundAsync() => PlayAsync("Button_Click_Mouse.m4a");
    public static Task PlayTurnoutClickSoundAsync() => PlayAsync("Button_Click_Fast.m4a");
    public static Task PlayLongPressSoundAsync() => PlayAsync("beep1.m4a");

    public static async Task PlayAsync(string filename) {
        var holder = await GetOrCreateAsync(filename).ConfigureAwait(false);
        var p = holder.Player;
        if (p.IsPlaying) p.Stop();
        p.Seek(0);
        p.Play();
    }

    private static async Task<PlayerHolder> GetOrCreateAsync(string filename) {
        if (Cache.TryGetValue(filename, out var existing)) return existing;

        // Read file fully into memory so the stream can safely outlive the CreatePlayer call.
        await using var file = await FileSystem.OpenAppPackageFileAsync(filename);
        var ms = new MemoryStream(capacity: (int)Math.Max(32 * 1024, file.CanSeek ? file.Length : 0));
        await file.CopyToAsync(ms).ConfigureAwait(false);
        ms.Position = 0;

        var player = Audio.CreatePlayer(ms, Audio.DefaultPlayerOptions);
        player.Loop = false;
        player.Volume = 1.0;

        var holder = new PlayerHolder(player, ms);
        return Cache.GetOrAdd(filename, holder);
    }

    // Optional: free everything (e.g., on app shutdown)
    public static void DisposeAll() {
        foreach (var kv in Cache) kv.Value.Dispose();
        Cache.Clear();
    }
}