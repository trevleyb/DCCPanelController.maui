using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionButtonTile : Tile, ITileInteractive {
    public ActionButtonTile(ActionButtonEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));
        if (Entity is ActionButtonEntity button) button.State = ButtonStateEnum.Unknown;
    }

    public SvgImage? SvgImage { get; protected set; }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is ActionButtonEntity button) {
            if (connectionService?.Client is { } client) {
                if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
                var newState = button.State switch {
                    ButtonStateEnum.Unknown => ButtonStateEnum.On,
                    ButtonStateEnum.On      => ButtonStateEnum.Off,
                    ButtonStateEnum.Off     => ButtonStateEnum.On,
                    _                       => ButtonStateEnum.Unknown,
                };
                button.SetState(newState, StateChangeSource.External);
            }
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ActionButtonEntity button) {
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("ButtonLarge", Entity.Rotation),
                ButtonSizeEnum.Small => SvgImages.GetImage("ButtonSmall", Entity.Rotation),
                _                    => SvgImages.GetImage("button", Entity.Rotation),
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