using DCCClients;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
        entity.PropertyChanged += (sender, args) => Console.WriteLine($"Turnout property was Changed: {args.PropertyName}");
    }

    private TurnoutStateEnum State {
        get;
        set => SetField(ref field, value);
    } = TurnoutStateEnum.Unknown;

    public void Interact(IDccClient? client) {
        ClickSounds.PlayTurnoutClickSound();
        State = State switch {
            TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
            TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
            _                        => TurnoutStateEnum.Unknown
        };

        if (client is not null && Entity is TurnoutEntity { Turnout.DccAddress: {} address } turnoutEntity) {
            client.SendTurnoutCmdAsync(address, State != TurnoutStateEnum.Closed);
        }
    }

    public void Secondary(IDccClient? client) { }

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