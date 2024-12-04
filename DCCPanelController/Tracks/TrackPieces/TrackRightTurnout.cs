using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackRightTurnout : TrackTurnoutBase, ITrackTurnout, ITrackSymbol, ITrackPiece {
    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Right Hand Turnout")]
    private string _name = "Right Turnout";
    
    protected override void Setup() {
        SetTrackSymbol("TurnoutR1");
        AddImageSourceAndRotation(TrackStyleImage.Normal,   "TurnoutR1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Straight,  "TurnoutR2", (0 ,0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Diverging, "TurnoutR3", (0, 0), (90, 90), (180, 180), (270, 270));
    }
    
    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }
        _clickSoundPlayer?.Play();
    }
}