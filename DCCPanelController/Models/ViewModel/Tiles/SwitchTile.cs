using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class SwitchTile : Tile, ITileInteractive {
    public SwitchTile(SwitchEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(SwitchEntity.State));
        VisualProperties.Add(nameof(SwitchEntity.SwitchStyle));
        
        if (Entity is SwitchEntity switchEntity && switchEntity.Light is {} light) {
            light.PropertyChanged += (sender, args) => {
                switchEntity.State = light.State ? ButtonStateEnum.On : ButtonStateEnum.Off;
            };
        }
    }

    public async Task Interact(ConnectionService? connectionService) {
        if (connectionService is not null && Entity is SwitchEntity switchEntity) {
            if (UseClickSounds) await ClickSounds.PlaySwitchClickSoundAsync();
            switchEntity.State = switchEntity.State switch {
                ButtonStateEnum.Unknown => ButtonStateEnum.On,
                ButtonStateEnum.On      => ButtonStateEnum.Off,
                ButtonStateEnum.Off     => ButtonStateEnum.On,
                _                       => ButtonStateEnum.Unknown
            };
            if (connectionService.Client is { } client && switchEntity is {Light: not null } )  await client.SendLightCmdAsync(switchEntity.Light, switchEntity.State == ButtonStateEnum.On);
        }
    }

    public async Task Secondary(ConnectionService? connectionService) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
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
            svgImage.SetAttribute(SvgElementType.Button, switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.Parent?.LightOnColor ?? Colors.Green,
                ButtonStateEnum.Off => switchEntity.Parent?.LightOffColor ?? Colors.Red,
                _                   => switchEntity.Parent?.LightOffColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, switchEntity.State switch {
                ButtonStateEnum.On  => switchEntity.Parent?.LightOnBorderColor ?? Colors.Green,
                ButtonStateEnum.Off => switchEntity.Parent?.LightOffBorderColor ?? Colors.Gray,
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