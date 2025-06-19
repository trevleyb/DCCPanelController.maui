using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TurnoutEntity.State));
        if (Entity is TurnoutEntity turnoutEntity && turnoutEntity.Turnout is {} turnout) {
            turnout.PropertyChanged += (sender, args) => {
                turnoutEntity.State = turnout.State;
            };
        }
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (connectionService is not null && Entity is TurnoutEntity { Turnout: not null } turnout) {
            if (UseClickSounds) await ClickSounds.PlayTurnoutClickSoundAsync();
            var newState = turnout.State switch {
                TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
                TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
                TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
                _                        => TurnoutStateEnum.Unknown
            };
            turnout.SetState(newState, StateChangeSource.Internal);
            if (connectionService.Client is { } client) {
                await client.SendTurnoutCmdAsync(turnout.Turnout, newState != TurnoutStateEnum.Closed);
            }
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) {
        return false;
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        if (Entity is TurnoutEntity { Turnout: not null } turnout) {
            var imageName = turnout.State switch {
                TurnoutStateEnum.Unknown => trackName + "Unknown",
                TurnoutStateEnum.Closed  => trackName + "Straight",
                TurnoutStateEnum.Thrown  => trackName + "Diverging",
                _                        => trackName + "Unknown"
            };
            return base.CreateTrackTile(imageName, Entity.Rotation);
        }
        return base.CreateTrackTile(trackName + "Unknown", Entity.Rotation);
    }
}