using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLabelCircle(Panel? parent = null) : TrackLabelBase(parent), ITrackSymbol, ITrack {
    public TrackLabelCircle() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackLabelCircle>(parent);
    }

    [ObservableProperty]
    private string _name = "Circle wth Image";

    protected override void Setup() {
        Layer = 2;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
    }
}