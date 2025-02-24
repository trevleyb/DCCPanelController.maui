using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackButton(Panel? parent = null) : TrackButtonBase(parent), ITrack, ITrackButton, ITrackSymbol {

    [ObservableProperty] [property: EditableBoolProperty(Name = "IsEnabled", Description = "Is this button active and Enabled?")]
    private bool _isEnabled = true;

    [ObservableProperty] private string _name = "Button";
    
    public TrackButton() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackButton>(parent);
    }

    protected override void Setup() {
        Layer = 2;
        Name = "Button";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Active, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Active, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.InActive, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.InActive, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}