using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TurnoutTile : TrackTile, ITileInteractive {
    protected TurnoutTile(TurnoutEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
            .Track(nameof(TurnoutEntity.State), () => entity.State)
            .Track(nameof(TurnoutEntity.TurnoutStyle), () => entity.TurnoutStyle)
            .Track(nameof(TurnoutEntity.TrackNotSelectedColor), () => entity.TrackNotSelectedColor);

        if (Entity is TurnoutEntity turnoutEntity) {
            if (turnoutEntity.Turnout is { } turnout) {
                turnoutEntity.State = turnout.State;
                turnout.StateChanged += (_, state) => turnoutEntity.State = state;
            } else turnoutEntity.State = TurnoutStateEnum.Unknown;
        }
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is TurnoutEntity { Turnout: { } turnout } turnoutEntity) {
            if (connectionService?.Client is { } client) {
                if (UseClickSounds) await ClickSounds.PlayTurnoutClickSoundAsync();
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
                    TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
                    _                       => TurnoutStateEnum.Closed,
                };

                await client.SendTurnoutCmdAsync(turnout, newState != TurnoutStateEnum.Closed);
                turnoutEntity.SetState(newState, StateChangeSource.Internal); // not sure we do this - leave it to messages?
                return true;
            }
        }

        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    protected new Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        if (Entity is TurnoutEntity turnout) {
            var imageName = turnout.State switch {
                TurnoutStateEnum.Unknown => trackName + "Unknown",
                TurnoutStateEnum.Closed  => trackName + "Straight",
                TurnoutStateEnum.Thrown  => trackName + "Diverging",
                _                        => trackName + "Unknown",
            };

            if (turnout.TurnoutStyle == TurnoutStyleEnum.NoBranch) imageName += "alt";

            var style = new SvgStyleBuilder();

            if (!IsDesignMode) {
                if (IsPath && turnout.State == TurnoutStateEnum.Thrown) {
                    var color = Entity.Parent?.ShowPathColor ?? Colors.CornflowerBlue.WithAlpha(HighlightColorAlpha);
                    style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(color).Visible());
                } else if (IsOccupied) {
                    var color = Entity.Parent?.OccupiedColor ?? Colors.Tomato.WithAlpha(HighlightColorAlpha);
                    style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(color).Visible());
                } else {

                    if (turnout.State == TurnoutStateEnum.Closed) {
                        // If the current turnout is CLOSED (ie: we are passing through), then the diverging part of the 
                        // turnout should match the colors and style of the connected track. 
                        // ---------------------------------------------------------------------------------------------
                        if (turnout.GetNeighbourEntity(ConnectionType.Diverging) is TrackEntity { } neighbor) {
                            if (neighbor.IsBranchLine) {
                                style.Add(e => e.WithName(SvgElementType.BorderDiverging).Hidden());
                                style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(neighbor.TrackColor ?? neighbor.Parent?.BranchLineColor ?? Colors.Gray));
                            } else {
                                style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(neighbor.TrackColor ?? neighbor.Parent?.BranchLineColor ?? Colors.Gray));
                                style.Add(e => e.WithName(SvgElementType.BorderDiverging).WithColor(neighbor.TrackBorderColor ?? neighbor.Parent?.MainlineBorderColor ?? Colors.Gray));
                            }
                        }
                    } else if (turnout.State == TurnoutStateEnum.Thrown) {
                        // If, however, the turnout is Thrown, or Diverging, then the diverging part should match the other 
                        // track color and style. 
                        if (turnout.GetNeighbourEntity(ConnectionType.Closed) is TrackEntity { } neighbor) {
                            if (neighbor.IsBranchLine) {
                                style.Add(e => e.WithName(SvgElementType.BorderDiverging).Hidden());
                                style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(neighbor.TrackColor ?? neighbor.Parent?.BranchLineColor ?? Colors.Gray));
                            } else {
                                style.Add(e => e.WithName(SvgElementType.TrackDiverging).WithColor(neighbor.TrackColor ?? neighbor.Parent?.BranchLineColor ?? Colors.Gray));
                                style.Add(e => e.WithName(SvgElementType.BorderDiverging).WithColor(neighbor.TrackBorderColor ?? neighbor.Parent?.MainlineBorderColor ?? Colors.Gray));
                            }
                        }
                    }
                }
            }

            var trackTurnout = base.CreateTrackTile(imageName, Entity.Rotation, style.Build());
        return trackTurnout;
    }
    return base.CreateTrackTile(trackName + "Unknown", Entity.Rotation);
}

}