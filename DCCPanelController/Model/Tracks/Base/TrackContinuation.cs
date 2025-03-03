using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackContinuation : TrackPiece, ITrackPiece {
    [ObservableProperty]
    [property: EditableTrackImage(Name = "Track Style", Group = "Attributes", Description = "Style of this track piece", TrackTypes = new[] { TrackStyleImageEnum.Arrow, TrackStyleImageEnum.Lines }, Order = 5)]
    private TrackStyleImageEnum _trackImageEnum = TrackStyleImageEnum.Arrow;

    protected TrackContinuation(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline, TrackStyleImageEnum style = TrackStyleImageEnum.Arrow) : base(parent) {
        TrackTypeEnum = styleTypeEnum;
        TrackImageEnum = style;
    }

    protected TrackContinuation(Panel? parent = null) : base(parent) { }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }
}