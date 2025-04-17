using CommunityToolkit.Maui.Converters;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class RouteTile : Tile, ITileInteractive {
    public RouteTile(RouteEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
        VisualProperties.Add(nameof(ButtonEntity.ButtonSize));
    }

    private RouteStateEnum State {
        get;
        set => SetField(ref field, value);
    } = RouteStateEnum.Unknown;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is RouteEntity button) {
            var svgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("routeLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("route", Entity.Rotation)
            };
            svgImage.SetAttribute(SvgElementType.Button, State switch {
                RouteStateEnum.Active   => button.Parent?.ButtonOnColor ?? Colors.Green,
                RouteStateEnum.Inactive => button.Parent?.ButtonOffColor ?? Colors.Red,
                _                       => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, State switch {
                RouteStateEnum.Active   => button.Parent?.ButtonOnBorder ?? Colors.Black,
                RouteStateEnum.Inactive => button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                       => button.Parent?.ButtonBorder ?? Colors.Black
            });

            var image = new Image {
                Source = svgImage.AsImageSource(0, DefaultScaleFactor),
            };
            image.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), BindingMode.TwoWay, source: Entity));
            return image;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("route").AsImage();
    }

    public void Interact() {
        ClickSounds.PlayButtonClickSound();
        State = State switch {
            RouteStateEnum.Unknown  => RouteStateEnum.Active,
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Unknown
        };
    }

    public void Secondary() { }
}