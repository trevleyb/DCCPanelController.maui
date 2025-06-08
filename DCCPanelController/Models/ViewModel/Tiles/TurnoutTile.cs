using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
        if (Entity is TurnoutEntity { Turnout: {} turnout }) {
            if (turnout.Id == entity?.Turnout?.Id) State = turnout.State; 
            turnout.PropertyChanged += (sender, args) => { 
                Console.WriteLine($"Turnout property was Changed: {turnout.Id}:{turnout.Name} is {turnout.State}  {args.PropertyName}");
                if (turnout.Id == entity?.Turnout?.Id) {
                    Console.WriteLine($"Turnout MATCHED: {turnout.Id}:{turnout.Name} {turnout.State}");
                    State = turnout.State;
                }
            };
        }
    }

    private TurnoutStateEnum State {
        get;
        set => SetField(ref field, value);
    } = TurnoutStateEnum.Unknown;

    public async Task Interact(ConnectionService? connectionService) {
        ClickSounds.PlayTurnoutClickSound();
        State = State switch {
            TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
            TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
            _                        => TurnoutStateEnum.Unknown
        };

        if (connectionService is not null && Entity is TurnoutEntity { } turnoutEntity) {
            await connectionService.SendTurnoutCmdAsync(turnoutEntity.Turnout, State != TurnoutStateEnum.Closed);
        }
    }

    public async Task Secondary(ConnectionService? connectionService) { }

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