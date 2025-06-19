using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionRouteTile : Tile, ITileInteractive {
    public ActionRouteTile(RouteEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(ButtonEntity.State));
        VisualProperties.Add(nameof(ButtonEntity.ButtonSize));
        if (Entity is RouteEntity routeEntity && routeEntity.Route is {} route) {
            route.PropertyChanged += (sender, args) => {
                routeEntity.State = route.State;
            };
        }
    }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (UseClickSounds) await ClickSounds.PlayRouteClickSoundAsync();
        if (Entity is RouteEntity route) {
            route.State = route.State switch {
                RouteStateEnum.Unknown  => RouteStateEnum.Active,
                RouteStateEnum.Active   => RouteStateEnum.Inactive,
                RouteStateEnum.Inactive => RouteStateEnum.Active,
                _                       => RouteStateEnum.Unknown
            };

            if (connectionService is not null && Entity is RouteEntity { Route.Id: { } id } routeEntity) {
                if (connectionService.Client is { } client) await client.SendRouteCmdAsync(routeEntity.Route, routeEntity.State != RouteStateEnum.Inactive);
            }
            return true;
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) {
        return false;
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is RouteEntity route) {
            var svgImage = route.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("routeLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("route", Entity.Rotation)
            };
            svgImage.SetAttribute(SvgElementType.Button, route.State switch {
                RouteStateEnum.Active   => route.Parent?.ButtonOnColor ?? Colors.Green,
                RouteStateEnum.Inactive => route.Parent?.ButtonOffColor ?? Colors.Red,
                _                       => route.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, route.State switch {
                RouteStateEnum.Active   => route.Parent?.ButtonOnBorder ?? Colors.Black,
                RouteStateEnum.Inactive => route.Parent?.ButtonOffBorder ?? Colors.Black,
                _                       => route.Parent?.ButtonBorder ?? Colors.Black
            });

            var image = new Image {
                Source = svgImage.AsImageSource(0, DefaultScaleFactor)
            };
            image.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), BindingMode.TwoWay, source: Entity));
            return image;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("route").AsImage();
    }
}