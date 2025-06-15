using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ButtonTile : Tile, ITileInteractive {
    public ButtonTile(ButtonEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
        VisualProperties.Add(nameof(ButtonEntity.ButtonSize));
    }

    private ButtonStateEnum State {
        get;
        set => SetField(ref field, value);
    } = ButtonStateEnum.Unknown;

    public async Task Interact(ConnectionService? connectionService) {
        if (connectionService is not null && Entity is ButtonEntity button) {

            if (UseClickSounds) ClickSounds.PlayButtonClickSound();
            State = State switch {
                ButtonStateEnum.Unknown => ButtonStateEnum.On,
                ButtonStateEnum.On      => ButtonStateEnum.Off,
                ButtonStateEnum.Off     => ButtonStateEnum.On,
                _                       => ButtonStateEnum.Unknown
            };
            ActionApplyButton.ApplyButtonActions(button, State, connectionService);
        }
    }

    public async Task Secondary(ConnectionService? connectionService) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ButtonEntity button) {
            var svgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("ButtonLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("button", Entity.Rotation)
            };
            svgImage.SetAttribute(SvgElementType.Button, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.Parent?.ButtonOffBorder ?? Colors.Black,
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