using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackStraight : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Straight Track")]
    private string _name = "Straight";

    protected override void Setup() {
        SetTrackSymbol("Straight1");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Straight1", (0, -90), (90 ,0), (180 ,90), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "TurnoutR2", (45, 0), (135 ,90), (225 ,0), (315, -90));
    }
}