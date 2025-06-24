using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TurnoutEntity.State));
        VisualProperties.Add(nameof(TurnoutEntity.TrackNotSelectedColor));
        VisualProperties.Add(nameof(TurnoutEntity.TurnoutStyle));
        if (Entity is TurnoutEntity turnoutEntity && turnoutEntity.Turnout is { } turnout) {
            turnout.PropertyChanged += (sender, args) => { turnoutEntity.State = turnout.State; };
        }
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is TurnoutEntity { } turnout) {
            if (UseClickSounds) await ClickSounds.PlayTurnoutClickSoundAsync();
            var newState = turnout.State switch {
                TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
                TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
                TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
                _                        => TurnoutStateEnum.Unknown
            };
            turnout.SetState(newState, StateChangeSource.Internal);
            if (connectionService?.Client is { } client && turnout.Turnout is not null) {
                await client.SendTurnoutCmdAsync(turnout.Turnout, newState != TurnoutStateEnum.Closed);
            } 
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) {
        return false;
    }

    new protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        if (Entity is TurnoutEntity turnout) {
            var imageName = turnout.State switch {
                TurnoutStateEnum.Unknown => trackName + "Unknown",
                TurnoutStateEnum.Closed  => trackName + "Straight",
                TurnoutStateEnum.Thrown  => trackName + "Diverging",
                _                        => trackName + "Unknown"
            };
            if (turnout.TurnoutStyle == TurnoutStyleEnum.NoBranch) imageName += "alt";
            
            var style = new SvgStyleBuilder();
            if (turnout.TrackNotSelectedColor is { } divergingColor) {
                style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(divergingColor ?? turnout?.Parent?.DivergingColor ?? Colors.Gray));
            }
            
            var trackTurnout = base.CreateTrackTile(imageName, Entity.Rotation, style.Build());
            return trackTurnout;
        }
        return base.CreateTrackTile(trackName + "Unknown", Entity.Rotation);
    }
}