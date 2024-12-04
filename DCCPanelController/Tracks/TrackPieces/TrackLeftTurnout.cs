using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks;

public partial class TrackLeftTurnout : TrackTurnoutBase, ITrackTurnout, ITrackSymbol, ITrackPiece {
    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Left hand Turnout")]
    private string _name = "Left Turnout";

    protected override void Setup() {
        SetTrackSymbol("TurnoutL1");
        AddImageSourceAndRotation(TrackStyleImage.Normal,   "TurnoutL1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Straight,  "TurnoutL2", (0 ,0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Diverging, "TurnoutL3", (0, 0), (90, 90), (180, 180), (270, 270));
    }
    
    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }

        _clickSoundPlayer?.Play();
    }
}