using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks;

public class TrackRightTurnout : TrackPiece, ITrackTurnout, ITrackSymbol {

    private IAudioPlayer? _clickSoundPlayer;

    protected override void Setup() {
        Name = "Right Turnout";
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   UnknownState, "TurnoutR1", 0);
        AddTrackImage(90,  UnknownState, "TurnoutR1", 90);
        AddTrackImage(180, UnknownState, "TurnoutR1", 180);
        AddTrackImage(270, UnknownState, "TurnoutR1", 270);

        AddTrackImage(0,   "Straight", "TurnoutR2", 0);
        AddTrackImage(90,  "Straight", "TurnoutR2", 90);
        AddTrackImage(180, "Straight", "TurnoutR2", 180);
        AddTrackImage(270, "Straight", "TurnoutR2", 270);

        AddTrackImage(0,   "Diverging", "TurnoutR3", 0);
        AddTrackImage(90,  "Diverging", "TurnoutR3", 90);
        AddTrackImage(180, "Diverging", "TurnoutR3", 180);
        AddTrackImage(270, "Diverging", "TurnoutR3", 270);
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