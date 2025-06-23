using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionButtonTile : Tile, ITileInteractive {
    public ActionButtonTile(ButtonEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(ButtonEntity.State));
        VisualProperties.Add(nameof(ButtonEntity.ButtonSize));
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is ButtonEntity button) {

            if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
            var newState = button.State switch {
                ButtonStateEnum.Unknown => ButtonStateEnum.On,
                ButtonStateEnum.On      => ButtonStateEnum.Off,
                ButtonStateEnum.Off     => ButtonStateEnum.On,
                _                       => ButtonStateEnum.Unknown
            };
            button.SetState(newState, StateChangeSource.External);
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) {
        return false;
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ButtonEntity button) {
            var svgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("ButtonLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("button", Entity.Rotation)
            };
            svgImage.SetAttribute(SvgElementType.Button, button.State switch {
                ButtonStateEnum.On  => button.ColorOn  ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, button.State switch {
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