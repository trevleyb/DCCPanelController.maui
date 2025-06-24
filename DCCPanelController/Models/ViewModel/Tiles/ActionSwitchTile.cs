using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionSwitchTile : Tile, ITileInteractive {
    public SvgImage? SvgImage { get; protected set; }

    public ActionSwitchTile(SwitchEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(SwitchEntity.State));
        VisualProperties.Add(nameof(SwitchEntity.SwitchStyle));

        if (Entity is SwitchEntity switchEntity && switchEntity.Light is { } light) {
            light.PropertyChanged += (sender, args) => { switchEntity.State = light.State ? ButtonStateEnum.On : ButtonStateEnum.Off; };
        }
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is SwitchEntity switchEntity) {
            if (UseClickSounds) await ClickSounds.PlaySwitchClickSoundAsync();
            switchEntity.State = switchEntity.State switch {
                ButtonStateEnum.Unknown => ButtonStateEnum.On,
                ButtonStateEnum.On      => ButtonStateEnum.Off,
                ButtonStateEnum.Off     => ButtonStateEnum.On,
                _                       => ButtonStateEnum.Unknown
            };
            if (connectionService?.Client is { } client && switchEntity is { Light: not null }) await client.SendLightCmdAsync(switchEntity.Light, switchEntity.State == ButtonStateEnum.On);
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
        if (Entity is SwitchEntity switchEntity) {
            SvgImage = switchEntity.SwitchStyle switch {
                SwitchStyleEnum.Light  => SvgImages.GetImage("light", Entity.Rotation),
                SwitchStyleEnum.Button => SvgImages.GetImage("button", Entity.Rotation),
                _ => switchEntity.State switch {
                    ButtonStateEnum.On  => SvgImages.GetImage("switchon", Entity.Rotation),
                    ButtonStateEnum.Off => SvgImages.GetImage("switchoff", Entity.Rotation),
                    _                   => SvgImages.GetImage("switch", Entity.Rotation)
                }
            };

            var buttonColor = switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.ColorOn ?? switchEntity.Parent?.LightOnColor ?? Colors.Green,
                ButtonStateEnum.Off => switchEntity.ColorOff ?? switchEntity.Parent?.LightOffColor ?? Colors.Red,
                _                   => switchEntity.Parent?.ButtonColor ?? Colors.Gray
            };

            var buttonOutline = switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.ColorOnBorder ?? switchEntity.Parent?.LightOnBorderColor ?? Colors.Black,
                ButtonStateEnum.Off => switchEntity.ColorOffBorder ?? switchEntity.Parent?.LightOffBorderColor ?? Colors.Black,
                _                   => switchEntity.Parent?.ButtonBorder ?? Colors.Black
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
        if (Entity is SwitchEntity switchEntity) {
            SvgImage svgImage;
            if (switchEntity.SwitchStyle == SwitchStyleEnum.Light) {
                svgImage = SvgImages.GetImage("light", Entity.Rotation);
            } else if (switchEntity.SwitchStyle == SwitchStyleEnum.Button) {
                svgImage = SvgImages.GetImage("button", Entity.Rotation);
            } else {
                svgImage = switchEntity.State switch {
                    ButtonStateEnum.On  => SvgImages.GetImage("switchon", Entity.Rotation),
                    ButtonStateEnum.Off => SvgImages.GetImage("switchoff", Entity.Rotation),
                    _                   => SvgImages.GetImage("switch", Entity.Rotation)
                };
            }
            svgImage.SetAttributeFillColor(SvgElementType.Button, switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.ColorOn ?? switchEntity.Parent?.LightOnColor ?? Colors.Green,
                ButtonStateEnum.Off => switchEntity.ColorOff ?? switchEntity.Parent?.LightOffColor ?? Colors.Red,
                _                   => switchEntity.Parent?.LightOffColor ?? Colors.Gray
            });
            svgImage.SetAttributeFillColor(SvgElementType.ButtonOutline, switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.ColorOnBorder ?? switchEntity.Parent?.LightOnBorderColor ?? Colors.Green,
                ButtonStateEnum.Off => switchEntity.ColorOffBorder ?? switchEntity.Parent?.LightOffBorderColor ?? Colors.Gray,
                _                   => switchEntity.Parent?.LightOffBorderColor ?? Colors.Gray
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
        if (Entity is SwitchEntity button) {
            return button.SwitchStyle switch {
                SwitchStyleEnum.Light  => SvgImages.GetImage("light").AsImage(),
                SwitchStyleEnum.Button => SvgImages.GetImage("button").AsImage(),
                _                      => SvgImages.GetImage("switch").AsImage()
            };
        }
        return SvgImages.GetImage("switchon").AsImage();
    }
}