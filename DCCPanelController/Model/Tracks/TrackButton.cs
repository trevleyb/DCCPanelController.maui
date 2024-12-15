using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackButton(Panel? parent = null) : TrackButtonBase(parent), ITrackPiece, ITrackButton, ITrackSymbol {

    public TrackButton() : this(null) { }       

    [ObservableProperty] [property: EditableBoolProperty(Name = "IsEnabled", Description = "Is this button active and Enabled?")]
    private bool _isEnabled = true;

    protected override void Setup() {
        Layer = 2;
        Name= "Button";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "Button",(0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Button",(0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "ButtonCorner",(45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Active, "Button",(0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Active, "ButtonCorner",(45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.InActive, "Button",(0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.InActive, "ButtonCorner",(45, 0), (135, 90), (225, 180), (315, 270));
    }

    protected override void PushButtonAction(bool isActive) {
        Console.WriteLine($"Button '{Name}' is {(isActive ? "ACTIVE" : "INACTIVE")}");
    }
}