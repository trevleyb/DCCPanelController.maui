using System.Text;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract partial class TurnoutTile : TrackTile, ITileInteractive {

    private TurnoutStateEnum State {
        get;
        set => SetField(ref field, value);
    } = TurnoutStateEnum.Unknown;

    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        var imageName = State switch {
            TurnoutStateEnum.Unknown => trackName + "Unknown",
            TurnoutStateEnum.Closed  => trackName + "Straight",
            TurnoutStateEnum.Thrown  => trackName + "Diverging",
            _                        => trackName + "Unknown"
        };
        return base.CreateTrackTile(imageName, Entity.Rotation);
    }

    public void Interact() {
        ClickSounds.PlayTurnoutClickSound();
        State = State switch {
            TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
            TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
            _                        => TurnoutStateEnum.Unknown
        };
    }

    public void Secondary() { }
}