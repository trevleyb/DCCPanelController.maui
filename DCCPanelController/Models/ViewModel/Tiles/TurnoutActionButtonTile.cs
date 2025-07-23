using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TurnoutActionButtonTile : Tile, ITileInteractive {
    private TurnoutEntity? _turnout = null;
    public SvgImage? SvgImage { get; protected set; }

    public TurnoutActionButtonTile(TurnoutButtonEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));
        RegisterForTurnoutEvents();
    }

    new protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        base.OnPropertyChanged(sender,e);
        if (e.PropertyName == nameof(TurnoutButtonEntity.Turnout)) {
            if (_turnout is not null) _turnout.PropertyChanged -= TurnoutOnPropertyChanged;
            RegisterForTurnoutEvents();
        }
    }

    private void RegisterForTurnoutEvents() {
        // Find the related Turnout for this Tile and subscribe 
        // to its events so we can change states. 
        // -----------------------------------------------------------------
        if (Entity is TurnoutButtonEntity { Turnout: { } turnoutRef } entity) {
            _turnout = entity?.Parent?.GetTurnoutEntityByRef(turnoutRef);
            if (_turnout is not null) {
                _turnout.PropertyChanged += TurnoutOnPropertyChanged;
                UpdateEntityStateBasedOnTurnout();
            }
        }
    }
    
    private void TurnoutOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (Entity is TurnoutButtonEntity entity && _turnout is not null) {
            UpdateEntityStateBasedOnTurnout();
        }
    }

    private void UpdateEntityStateBasedOnTurnout() {
        if (Entity is TurnoutButtonEntity entity) {
            entity!.State = _turnout?.State switch {
                TurnoutStateEnum.Closed => entity.WhenNormal,
                TurnoutStateEnum.Thrown => entity.WhenThrown,
                _                       => ButtonStateEnum.Unknown
            };
        }
    }

    protected override void Cleanup() {
        if (_turnout is not null) _turnout.PropertyChanged -= TurnoutOnPropertyChanged;
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is TurnoutButtonEntity button) {
            if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
            _turnout?.ToggleState();
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) {
        return false;
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTileAsCanvas();
    }

    protected Microsoft.Maui.Controls.View? CreateTileAsCanvas() {
        if (Entity is TurnoutButtonEntity button) {
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("ButtonLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("button", Entity.Rotation)
            };

            var buttonColor = button.State switch {
                ButtonStateEnum.On  => button.ColorOn ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            };

            var buttonOutline = button.State switch {
                ButtonStateEnum.On  => button.ColorOnBorder ?? button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.ColorOffBorder ?? button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                   => button.Parent?.ButtonBorder ?? Colors.Black
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
        return CreateTileAsImage();
    }

    protected Microsoft.Maui.Controls.View? CreateTileAsImage() {
        if (Entity is TurnoutButtonEntity button) {
            var svgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("ButtonLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("button", Entity.Rotation)
            };
            svgImage.SetAttributeFillColor(SvgElementType.Button, button.State switch {
                ButtonStateEnum.On  => button.ColorOn ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttributeFillColor(SvgElementType.ButtonOutline, button.State switch {
                ButtonStateEnum.On  => button.ColorOnBorder ?? button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.ColorOffBorder ?? button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                   => button.Parent?.ButtonBorder ?? Colors.Black
            });

            var image = new Image {
                Source = svgImage.AsImageSource(0, DefaultScaleFactor)
            };
            image.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), BindingMode.TwoWay, source: Entity));
            return image;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("button").AsImage();
    }

}