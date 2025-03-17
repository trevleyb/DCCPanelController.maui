using System.Diagnostics;
using Plugin.Maui.Audio;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class ClickSounds {

    private static readonly Dictionary<string, IAudioPlayer> ClickSoundPlayers = new();
    public static void PlayButtonClickSound() => ClickSoundPlayer("Button_Click_Mouse.m4a")?.Play(); 
    public static void PlayTurnoutClickSound() => ClickSoundPlayer("Button_Click_Fast.m4a")?.Play();

    private static IAudioPlayer? ClickSoundPlayer(string filename) {
        if (ClickSoundPlayers?.ContainsKey(filename) ?? false) return ClickSoundPlayers[filename];
        try {
            var audioManager = AudioManager.Current;
            var clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync(filename).Result);
            ClickSoundPlayers?.Add(filename, clickSoundPlayer);
            return clickSoundPlayer;
        } catch (Exception ex) {
            Debug.WriteLine($"Error loading sound file: {filename}: {ex.Message}");
            return null;
        }
    }
}