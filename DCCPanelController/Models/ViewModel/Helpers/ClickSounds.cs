using Plugin.Maui.Audio;
using AudioManager = Plugin.Maui.Audio.AudioManager;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class ClickSounds {
    private static readonly Dictionary<string, IAudioPlayer> ClickSoundPlayers = new();

    public static async Task PlayButtonClickSoundAsync() {
        var clickSound = await ClickSoundPlayerAsync("Button_Click_Mouse.m4a");
        clickSound?.Play();
    }

    public static async Task PlayTurnoutClickSoundAsync() {
        var clickSound = await ClickSoundPlayerAsync("Button_Click_Fast.m4a");
        clickSound?.Play();
    }

    private static async Task<IAudioPlayer?> ClickSoundPlayerAsync(string filename) {
        if (ClickSoundPlayers?.ContainsKey(filename) ?? false) return ClickSoundPlayers[filename];
        try {
            var audioManager = new AudioManager();
            await using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            var clickSoundPlayer = audioManager.CreatePlayer(stream, AudioManager.Current.DefaultPlayerOptions);
            ClickSoundPlayers?.Add(filename, clickSoundPlayer);
            return clickSoundPlayer;
        } catch (Exception ex) {
            Console.WriteLine($"Error loading sound file: {filename}: {ex.Message}");
            return null;
        }
    }
    
}