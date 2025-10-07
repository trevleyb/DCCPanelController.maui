using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ScriptButtonTile : Tile, ITileInteractive {
    private bool _isActionRunning = false;

    public ScriptButtonTile(ScriptButtonEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
            .Track(nameof(ScriptButtonEntity.State), () => entity.State)
            .Track(nameof(ScriptButtonEntity.ButtonStyle), () => entity.ButtonStyle)
            .Track(nameof(ScriptButtonEntity.ButtonSize), () => entity.ButtonSize);
    }

    public SvgImage? SvgImage { get; protected set; }

    public async Task<bool> Interact(ConnectionService? connectionService) {
        if (Entity is ScriptButtonEntity button && !_isActionRunning) {
            _isActionRunning = true;
            button.State = ButtonStateEnum.Off;
            if (UseClickSounds) await ClickSounds.PlayButtonClickSoundAsync();
            switch (button.Scriptname.ToLowerInvariant()) {
            case"none":
                break;

            case"set all turnouts to default":
                connectionService?.SetTurnoutsToDefaultState();
                break;
            }

            await Task.Delay(250);
            button.State = ButtonStateEnum.On;
            _isActionRunning = false;
            return true;
        }

        return false;
    }

    public async Task<bool> Secondary(ConnectionService? connectionService) => false;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ScriptButtonEntity button) {
            var size = button.ButtonStyle == ButtonStyleEnum.Round ? "Round" : "Square";
            SvgImage = button.ButtonSize switch {
                ButtonSizeEnum.Large => SvgImages.GetImage($"Button{size}Large", Entity.Rotation),
                ButtonSizeEnum.Small => SvgImages.GetImage($"Button{size}Small", Entity.Rotation),
                _                    => SvgImages.GetImage($"Button{size}", Entity.Rotation),
            };

            var buttonColor = button.State switch {
                ButtonStateEnum.On  => button.ColorOn ?? button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.ColorOff ?? button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.ColorUnknown ?? button.Parent?.ButtonColor ?? Colors.Gray,
            };

            var buttonOutline = button.State switch {
                ButtonStateEnum.On  => button.ColorOnBorder ?? button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.ColorOffBorder ?? button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                   => button.ColorUnknownBorder ?? button.Parent?.ButtonBorder ?? Colors.Black,
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

        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}