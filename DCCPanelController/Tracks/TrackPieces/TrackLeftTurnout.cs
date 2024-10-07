using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks;

public class TrackLeftTurnout : TrackPiece, ITrackTurnout, ITrackSymbol {
    private IAudioPlayer? _clickSoundPlayer;
    
    protected override void Setup() {
        Name = "Left Turnout";
        SetTrackSymbol("TurnoutL1");
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   UnknownState, "TurnoutL1", 0);
        AddTrackImage(90,  UnknownState, "TurnoutL1", 90);
        AddTrackImage(180, UnknownState, "TurnoutL1", 180);
        AddTrackImage(270, UnknownState, "TurnoutL1", 270);

        AddTrackImage(0,   "Straight", "TurnoutL2", 0);
        AddTrackImage(90,  "Straight", "TurnoutL2", 90);
        AddTrackImage(180, "Straight", "TurnoutL2", 180);
        AddTrackImage(270, "Straight", "TurnoutL2", 270);

        AddTrackImage(0,   "Diverging", "TurnoutL3", 0);
        AddTrackImage(90,  "Diverging", "TurnoutL3", 90);
        AddTrackImage(180, "Diverging", "TurnoutL3", 180);
        AddTrackImage(270, "Diverging", "TurnoutL3", 270);
    }
    protected override void AddTrackStyles() {
        AddTrackStyle(UnknownState,"Mainline");
        AddTrackStyle("Straight","Mainline-Straight");
        AddTrackStyle("Diverging","Mainline-Diverging");
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }
        _clickSoundPlayer?.Play();
    }

}