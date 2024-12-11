using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLeftTurnout(Panel? parent = null) : TrackTurnoutBase(parent), ITrackTurnout, ITrackSymbol, ITrackPiece {
    private IAudioPlayer? _clickSoundPlayer;

    public TrackLeftTurnout() : this(null) { }
    protected override void Setup() {
        SetTrackSymbol("TurnoutL1");
        Name = "Left Turnout";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "TurnoutL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Straight, "TurnoutL2", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Diverging, "TurnoutL3", (0, 0), (90, 90), (180, 180), (270, 270));
    }
    
    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }

        _clickSoundPlayer?.Play();
    }
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }


}