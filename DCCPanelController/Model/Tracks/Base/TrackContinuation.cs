using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackContinuation : TrackPiece {

    // TODO: Make a Track Continuation have a link to another page. I should be able 
    //       to click on the continuation track and have it go to that new panel 
    //       or page. 
    
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