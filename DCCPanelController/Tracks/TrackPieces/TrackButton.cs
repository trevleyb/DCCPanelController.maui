using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackButton : TrackButtonBase, ITrackPiece, ITrackButton, ITrackSymbol {
    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty] [property: EditableBoolProperty(Name = "IsEnabled", Description = "Is this button active and Enabled?")]
    private bool _isEnabled = true;

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }

        _clickSoundPlayer?.Play();
    }

    protected override void Setup() {
        Layer = 2;
        Name= "Button";
        SetTrackSymbol("Button");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Button");
        AddImageSourceAndRotation(TrackStyleImage.Active, "Button");
        AddImageSourceAndRotation(TrackStyleImage.InActive, "Button");
    }
}