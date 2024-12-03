using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks;

public partial class TrackButton : TrackButtonBase, ITrackPiece, ITrackButton, ITrackSymbol {
    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Button Piece")]
    private string _name = "Button";

    [ObservableProperty]
    [property: EditableBoolProperty(Name = "IsEnabled", Description = "Is this button active and Enabled?")]
    private bool _isEnabled = true;
   
    [ObservableProperty]
    [property: EditableBoolProperty(Name = "State", Description = "Current State of the Button")]
    private bool _state = false;

    protected override void Setup() {
        Layer = 2;
        SetTrackSymbol("Button");
        AddImageSourceAndRotation(TrackStyleImage.Unknown,  "Button");
        AddImageSourceAndRotation(TrackStyleImage.Active,   "Button");
        AddImageSourceAndRotation(TrackStyleImage.InActive, "Button");
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }
        _clickSoundPlayer?.Play();
    }
}