using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionSwitchTile : Tile, ITileInteractive {
    public ActionSwitchTile(SwitchEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(SwitchEntity.State));
        VisualProperties.Add(nameof(SwitchEntity.SwitchStyle));

        if (Entity is SwitchEntity { Light: { } light } switchEntity) {
            switchEntity.State = ButtonStateEnum.Unknown;
            light.PropertyChanged += (sender, args) => { switchEntity.State = light.State ? ButtonStateEnum.On : ButtonStateEnum.Off; };
        }
    }

    public SvgImage? SvgImage { get; protected set; }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is SwitchEntity switchEntity) {
            if (connectionService?.Client is { } client) {
                if (UseClickSounds) await ClickSounds.PlaySwitchClickSoundAsync();
                switchEntity.State = switchEntity.State switch {
                    ButtonStateEnum.Unknown => ButtonStateEnum.On,
                    ButtonStateEnum.On      => ButtonStateEnum.Off,
                    ButtonStateEnum.Off     => ButtonStateEnum.On,
                    _                       => ButtonStateEnum.Unknown,
                };
                if (switchEntity is { Light: { } }) await client.SendLightCmdAsync(switchEntity.Light, switchEntity.State == ButtonStateEnum.On);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is SwitchEntity button) {
            SvgImage = button.SwitchStyle switch {
                SwitchStyleEnum.Light =>
                    button.ButtonSize switch {
                        ButtonSizeEnum.Large => SvgImages.GetImage("lightLarge", Entity.Rotation),
                        ButtonSizeEnum.Small => SvgImages.GetImage("lightSmall", Entity.Rotation),
                        _                    => SvgImages.GetImage("light", Entity.Rotation),
                    },
                SwitchStyleEnum.Button =>
                    button.ButtonSize switch {
                        ButtonSizeEnum.Large => SvgImages.GetImage("buttonLarge", Entity.Rotation),
                        _                    => SvgImages.GetImage("button", Entity.Rotation),
                    },
                _ => button.State switch {
                    ButtonStateEnum.On  => SvgImages.GetImage("switchon", Entity.Rotation),
                    ButtonStateEnum.Off => SvgImages.GetImage("switchoff", Entity.Rotation),
                    _                   => SvgImages.GetImage("switch", Entity.Rotation),
                },
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
            style.Add(e => e.WithName(SvgElementType.Indicator).WithColor(buttonColor));
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