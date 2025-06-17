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
        
        if (Entity is TurnoutEntity turnout) {
            Entity.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(TurnoutEntity.State)) {
                    Console.WriteLine($"TurnoutTile: {turnout.Id} {args.PropertyName} ==> Apply States");
                    //turnout?.TurnoutPanelActions.Apply(turnout,ConnectionService.Instance);
                    turnout.SetState(turnout.State, StateChangeSource.External);
                }
            };
        }
    }

    public async Task Interact(ConnectionService? connectionService) {
        if (connectionService is not null && Entity is TurnoutEntity { Turnout: not null } turnout) {
            if (UseClickSounds) await ClickSounds.PlayTurnoutClickSoundAsync();
            var newState = turnout.State switch {
                TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
                TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
                TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
                _                        => TurnoutStateEnum.Unknown
            };
            
            // User interaction is external change
            turnout.SetState(newState, StateChangeSource.Internal);
            
            // Send to physical layout
            if (connectionService.Client is { } client) {
                await client.SendTurnoutCmdAsync(turnout.Turnout, newState != TurnoutStateEnum.Closed);
            }

            
        }
    }

    public async Task Secondary(ConnectionService? connectionService) { }

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