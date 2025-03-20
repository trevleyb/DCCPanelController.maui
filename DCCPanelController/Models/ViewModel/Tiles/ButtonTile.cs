using CommunityToolkit.Maui.Converters;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class ButtonTile : Tile, ITileInteractive {
    public ButtonTile(ButtonEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
    }

    private ButtonStateEnum State {
        get;
        set => SetField(ref field, value);
    } = ButtonStateEnum.Unknown;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ButtonEntity button) {
            var svgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("button_large", Entity.Rotation),
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
                Scale = 1.5
            };
            image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            return image;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("button").AsImage();
    }

    public void Interact() {
        ClickSounds.PlayButtonClickSound();
        State = State switch {
            ButtonStateEnum.Unknown => ButtonStateEnum.On,
            ButtonStateEnum.On      => ButtonStateEnum.Off,
            ButtonStateEnum.Off     => ButtonStateEnum.On,
            _                       => ButtonStateEnum.Unknown
        };
    }

    public void Secondary() { }
}