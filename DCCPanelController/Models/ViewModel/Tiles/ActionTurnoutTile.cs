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
    private TurnoutEntity? _turnout;

    public ActionTurnoutTile(TurnoutButtonEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));
        RegisterForTurnoutEvents();

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
        if (Entity is TurnoutButtonEntity { IsEnabled: true } button) {
            if (connectionService?.Client is { } client) {
                if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
                button.Turnout?.ToggleState();
                return true;
            }
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    new protected void OnTilePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        base.OnTilePropertyChanged(sender, e);
        if (e.PropertyName == nameof(TurnoutButtonEntity.TurnoutID)) {
            if (_turnout is { }) _turnout.PropertyChanged -= TurnoutOnPropertyChanged;
            RegisterForTurnoutEvents();
        }
    }

    private void RegisterForTurnoutEvents() {
        // Find the related Turnout for this Tile and subscribe 
        // to its events so we can change states. 
        // -----------------------------------------------------------------
        if (Entity is TurnoutButtonEntity { TurnoutID: { } turnoutRef } entity) {
            _turnout = entity?.Parent?.GetTurnoutEntityByRef(turnoutRef);
            if (_turnout is { }) {
                _turnout.PropertyChanged += TurnoutOnPropertyChanged;
                UpdateEntityStateBasedOnTurnout();
            }
        }
    }

    private void TurnoutOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (Entity is TurnoutButtonEntity entity && _turnout is { }) {
            UpdateEntityStateBasedOnTurnout();
        }
    }

    private void UpdateEntityStateBasedOnTurnout() {
        if (Entity is TurnoutButtonEntity entity) {
            entity!.State = _turnout?.State switch {
                TurnoutStateEnum.Closed => entity.WhenNormal,
                TurnoutStateEnum.Thrown => entity.WhenThrown,
                _                       => ButtonStateEnum.Unknown,
            };
        }
    }

    protected override void Cleanup() {
        if (_turnout is { }) _turnout.PropertyChanged -= TurnoutOnPropertyChanged;
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TurnoutButtonEntity button) {
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("TurnoutLarge", Entity.Rotation),
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