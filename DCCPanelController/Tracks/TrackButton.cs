using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using Plugin.Maui.Audio;

namespace DCCPanelController.Tracks;

public class TrackButton : TrackPiece, ITrackButton, ITrackSymbol {

    private IAudioPlayer? _clickSoundPlayer;
    
    protected override void Setup() {
        Name = "Button";
        Layer = 2;
    }
    
    protected override void AddTrackImages() {
        base.AddTrackImages();
        AddTrackImage(0,UnknownState, "Button", 0);
        AddTrackImage(0,UnknownState, "Button", 0);
        AddTrackImage(0,UnknownState, "Button", 0);
        AddTrackImage(0,UnknownState, "Button", 0);
        
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);

        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
    }
    
    protected override void AddTrackStyles() {
        base.AddTrackStyles();
        AddTrackStyle(UnknownState,"Button-UnKnown");
        AddTrackStyle("Active","Button-Active");
        AddTrackStyle("InActive","Button-InActive");
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }
        _clickSoundPlayer?.Play();
    }
}