using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionRouteTile : Tile, ITileInteractive {
    public SvgImage? SvgImage { get; protected set; }

    public ActionRouteTile(RouteEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));
        if (Entity is RouteEntity routeEntity && routeEntity.Route is { } route) {
            route.PropertyChanged += (sender, args) => { routeEntity.State = route.State; };
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
        return CreateTileAsCanvas();
    }

    protected Microsoft.Maui.Controls.View? CreateTileAsCanvas() {
        if (Entity is RouteEntity route) {
            SvgImage = route.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("routeLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("route", Entity.Rotation)
            };

            var buttonColor = route.State switch {
                RouteStateEnum.Active   => route.ColorOn ?? route.Parent?.ButtonOnColor ?? Colors.Green,
                RouteStateEnum.Inactive => route.ColorOff ?? route.Parent?.ButtonOffColor ?? Colors.Red,
                _                       => route.Parent?.ButtonColor ?? Colors.Gray
            };

            var buttonOutline = route.State switch {
                RouteStateEnum.Active   => route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black,
                RouteStateEnum.Inactive => route.ColorOffBorder ?? route.Parent?.ButtonOffBorder ?? Colors.Black,
                _                       => route.Parent?.ButtonBorder ?? Colors.Black
            };

            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Button).WithColor(buttonColor));
            style.Add(e => e.WithName(SvgElementType.ButtonOutline).WithColor(buttonOutline));
            SvgImage.ApplyStyle(style.Build());
            SvgImage.SetAttributeLineColor(SvgElementType.Indicator, route.RouteIndicator ?? route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black);
            SvgImage.SetAttributeFillColor(SvgElementType.Indicator, route.RouteIndicator ?? route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black);

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
        if (Entity is RouteEntity route) {
            var svgImage = route.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("routeLarge", Entity.Rotation),
                _                    => SvgImages.GetImage("route", Entity.Rotation)
            };
            svgImage.SetAttributeFillColor(SvgElementType.Button, route.State switch {
                RouteStateEnum.Active   => route.ColorOn ?? route.Parent?.ButtonOnColor ?? Colors.Green,
                RouteStateEnum.Inactive => route.ColorOff ?? route.Parent?.ButtonOffColor ?? Colors.Red,
                _                       => route.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttributeFillColor(SvgElementType.ButtonOutline, route.State switch {
                RouteStateEnum.Active   => route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black,
                RouteStateEnum.Inactive => route.ColorOffBorder ?? route.Parent?.ButtonOffBorder ?? Colors.Black,
                _                       => route.Parent?.ButtonBorder ?? Colors.Black
            });
            svgImage.SetAttributeLineColor(SvgElementType.Indicator, route.RouteIndicator ?? route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black);
            svgImage.SetAttributeFillColor(SvgElementType.Indicator, route.RouteIndicator ?? route.ColorOnBorder ?? route.Parent?.ButtonOnBorder ?? Colors.Black);

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