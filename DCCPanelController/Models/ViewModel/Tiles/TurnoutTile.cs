using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
    }

    private TurnoutStateEnum State {
        get;
        set => SetField(ref field, value);
    } = TurnoutStateEnum.Unknown;

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

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        var imageName = State switch {
            TurnoutStateEnum.Unknown => trackName + "Unknown",
            TurnoutStateEnum.Closed  => trackName + "Straight",
            TurnoutStateEnum.Thrown  => trackName + "Diverging",
            _                        => trackName + "Unknown"
        };
        return base.CreateTrackTile(imageName, Entity.Rotation);
    }
}