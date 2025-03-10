using Plugin.Maui.Audio;

namespace DCCPanelController.Models.ViewModel.Helpers;

public static class ClickSounds {
    
    private static IAudioPlayer? _clickSoundPlayer;
    
    public static void PlayButtonClickSound() => ClickSound(ClickSoundType.Button);    
    public static void PlayTurnoutClickSound() => ClickSound(ClickSoundType.Turnout);
    
    private static void ClickSound(ClickSoundType clickSoundType = ClickSoundType.Turnout) {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = clickSoundType switch {
                ClickSoundType.Turnout => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result),
                ClickSoundType.Button  => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result),
                _                      => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result),
            };
            _clickSoundPlayer?.Play();
        }
    }
}

public enum ClickSoundType {
    Turnout,
    Button
}