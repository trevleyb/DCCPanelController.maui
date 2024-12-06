using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class Sample : TrackPieceBase, ITrackPiece {
    [ObservableProperty] [property: EditableStringProperty(Name = "Name (ID)", Description = "A Sample Track Piece")]
    private string _name = "Sample";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Sample");
    }

    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}