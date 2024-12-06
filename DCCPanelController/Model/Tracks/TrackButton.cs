using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

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
    
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}