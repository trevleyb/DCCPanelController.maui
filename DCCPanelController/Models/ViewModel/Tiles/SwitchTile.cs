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
        
        if (Entity is SwitchEntity { Light : { } light }) {
            if (light.Id == entity?.Light?.Id) State = light.State ? ButtonStateEnum.On : ButtonStateEnum.Off;
            light.PropertyChanged += (sender, args) => {
                if (light.Id == entity?.Light?.Id) {
                    State = light.State ? ButtonStateEnum.On : ButtonStateEnum.Off;
                }
            };
        }
    }

    private ButtonStateEnum State {
        get;
        set => SetField(ref field, value);
    } = ButtonStateEnum.Unknown;

    public async Task Interact(ConnectionService? connectionService) {
        ClickSounds.PlayButtonClickSound();
        State = State switch {
            ButtonStateEnum.Unknown => ButtonStateEnum.On,
            ButtonStateEnum.On      => ButtonStateEnum.Off,
            ButtonStateEnum.Off     => ButtonStateEnum.On,
            _                       => ButtonStateEnum.Unknown
        };
        if (connectionService is not null && Entity is SwitchEntity { Light: not null } switchEntity) {
            if (connectionService.Client is {} client) await client.SendLightCmdAsync(switchEntity.Light, State == ButtonStateEnum.On);
        }
    }

    public async Task Secondary(ConnectionService? connectionService) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is SwitchEntity button) {
            SvgImage svgImage;
            if (button.SwitchStyle == SwitchStyleEnum.Light) {
                svgImage = SvgImages.GetImage("light", Entity.Rotation);
            } else if (button.SwitchStyle == SwitchStyleEnum.Button) {
                svgImage = SvgImages.GetImage("button", Entity.Rotation);
            } else {
                svgImage = State switch {
                    ButtonStateEnum.On  => SvgImages.GetImage("switchon", Entity.Rotation),
                    ButtonStateEnum.Off => SvgImages.GetImage("switchoff", Entity.Rotation),
                    _                   => SvgImages.GetImage("switch", Entity.Rotation)
                };
            }
            svgImage.SetAttribute(SvgElementType.Button, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.Parent?.ButtonOffColor ?? Colors.Gray,
                _                   => button.Parent?.ButtonOnColor ?? Colors.Gray
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