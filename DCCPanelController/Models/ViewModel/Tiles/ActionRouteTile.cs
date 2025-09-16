using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ActionRouteTile : Tile, ITileInteractive {
    public ActionRouteTile(RouteEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(ActionButtonEntity.State));
        VisualProperties.Add(nameof(ActionButtonEntity.ButtonSize));
        if (Entity is RouteEntity { Route: { } route } routeEntity) {
            routeEntity.State = RouteStateEnum.Unknown;
            route.PropertyChanged += (sender, args) => { routeEntity.State = route.State; };
        }
    }

    public SvgImage? SvgImage { get; protected set; }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (connectionService?.Client is { } client) {
            if (UseClickSounds) await ClickSounds.PlayRouteClickSoundAsync();
            if (Entity is RouteEntity route) {
                route.State = route.State switch {
                    RouteStateEnum.Unknown  => RouteStateEnum.Active,
                    RouteStateEnum.Active   => RouteStateEnum.Inactive,
                    RouteStateEnum.Inactive => RouteStateEnum.Active,
                    _                       => RouteStateEnum.Unknown,
                };

                if (Entity is RouteEntity { Route.Id: { } id } routeEntity) {
                    await client.SendRouteCmdAsync(routeEntity.Route, routeEntity.State != RouteStateEnum.Inactive);
                }
                return true;
            }
        }
        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is RouteEntity button) {
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage("routeLarge", Entity.Rotation),
                ButtonSizeEnum.Small => SvgImages.GetImage("RouteSmall", Entity.Rotation),
                _                    => SvgImages.GetImage("route", Entity.Rotation),
            };

            var buttonColor = button.State switch {
                RouteStateEnum.Active   => button.ColorOn ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                RouteStateEnum.Inactive => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                       => button.ColorUnknown ?? button.Parent?.ButtonColor ?? Colors.Gray,
            };

            var buttonOutline = button.State switch {
                RouteStateEnum.Active   => button.ColorOnBorder ?? button.Parent?.ButtonOnBorder ?? Colors.Black,
                RouteStateEnum.Inactive => button.ColorOffBorder ?? button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                       => button.ColorUnknownBorder ?? button.Parent?.ButtonBorder ?? Colors.Black,
            };

            var indicatorColor = button.ShowIndicator ? button.ColorIndicator ?? AppleCrayonColors.GetContrastingTextColor(buttonColor) ?? Colors.White : buttonColor;

            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Button).WithColor(buttonColor));
            style.Add(e => e.WithName(SvgElementType.ButtonOutline).WithColor(buttonOutline));
            style.Add(e => e.WithName(SvgElementType.Indicator).WithColor(indicatorColor));
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