using System.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionTurnoutTile : Tile, ITileInteractive {
    public ActionTurnoutTile(TurnoutButtonEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));

        if (Entity is TurnoutButtonEntity button) {
            if (button.Turnout is { } turnout) {
                SetButtonState(button, turnout.State);
                turnout.StateChanged += (_, state) => SetButtonState(button, state);
            } else button.State = ButtonStateEnum.Unknown;
        }
    }

    private void SetButtonState(TurnoutButtonEntity button, TurnoutStateEnum state) {
        button.State = state switch {
            TurnoutStateEnum.Unknown => ButtonStateEnum.Unknown,
            TurnoutStateEnum.Closed  => button.WhenNormal,
            TurnoutStateEnum.Thrown  => button.WhenThrown,
            _                        => button.State
        };
    }
    
    public SvgImage? SvgImage { get; protected set; }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is TurnoutButtonEntity { IsEnabled: true, Turnout: {} turnout } button) {
            if (connectionService?.Client is { } client) {
                if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
                    TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
                    _                       => TurnoutStateEnum.Closed,
                };
                await client.SendTurnoutCmdAsync(turnout, turnout.State != TurnoutStateEnum.Closed);
                turnout.State = newState;
                return true;
            }
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;


    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TurnoutButtonEntity button) {
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("TurnoutLarge", Entity.Rotation),
                ButtonSizeEnum.Small => SvgImages.GetImage("TurnoutSmall", Entity.Rotation),
                _                    => SvgImages.GetImage("Turnout", Entity.Rotation),
            };

            var buttonColor = button.State switch {
                ButtonStateEnum.On  => button.ColorOn ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.ColorUnknown ?? button.Parent?.ButtonColor ?? Colors.Gray,
            };

            var buttonOutline = button.State switch {
                ButtonStateEnum.On  => button.ColorOnBorder ?? button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.ColorOffBorder ?? button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                   => button.ColorUnknownBorder ?? button.Parent?.ButtonBorder ?? Colors.Black,
            };

            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Button).WithColor(buttonColor));
            style.Add(e => e.WithName(SvgElementType.ButtonOutline).WithColor(buttonOutline));
            SvgImage.ApplyStyle(style.Build());

            var canvas = SvgImage.AsCanvas(SvgImage.Rotation, 1);
            canvas.HorizontalOptions = LayoutOptions.Fill;
            canvas.VerticalOptions = LayoutOptions.Fill;
            canvas.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
            canvas.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), BindingMode.TwoWay, source: Entity));

            var absoluteLayout = new AbsoluteLayout();
            AbsoluteLayout.SetLayoutBounds(canvas, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
            absoluteLayout.Children.Add(canvas);
            return absoluteLayout;
        }
        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}