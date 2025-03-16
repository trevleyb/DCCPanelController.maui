using System.Text;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract partial class TurnoutTile : TrackTile, ITileInteractive {
    private TurnoutStateEnum State {
        get;
        set => SetField(ref field, value);
    } = TurnoutStateEnum.Unknown;

    protected TurnoutTile(TurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(State));
    }

    new protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {

        var svgImage = State switch {
            TurnoutStateEnum.Unknown => SvgImages.GetImage(trackName+"Unknown", trackRotation),
            TurnoutStateEnum.Closed  => SvgImages.GetImage(trackName+"Straight", trackRotation),
            TurnoutStateEnum.Thrown  => SvgImages.GetImage(trackName+"Diverging", trackRotation),
            _                        => null
        };

        if (svgImage is null) return null;
        var style = SetDefaultStyles();
        svgImage.ApplyStyle(style.Build());
        
        var image = new Image {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Scale = 1.5
        };

        image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
        return image;
    }

    public void Interact() {
        ClickSounds.PlayTurnoutClickSound();
        State = State switch {
            TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown  => TurnoutStateEnum.Closed,
            TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
            _                        => TurnoutStateEnum.Unknown
        };
    }

    public void Secondary() { }
}