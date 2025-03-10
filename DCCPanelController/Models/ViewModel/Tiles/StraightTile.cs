using System.Net.Http.Headers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class StraightTile(Entity entity, double gridSize) : TrackTile(entity, gridSize), ITileInteractive {

    public override void CreateTile() {
        if (Entity is StraightEntity straight) {
            var svgImage = SvgImages.GetImage("straight", straight.Rotation);

            var style = SetDefaultStyles();
            svgImage.ApplyStyle(style.Build());
            var image = new Image {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Scale = 1.5
            };
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            SetContent(image);
        }
    }

    public void Interact() {
        if (Entity is StraightEntity straight) {
            straight.TrackType = straight.TrackType == TrackTypeEnum.MainLine ? TrackTypeEnum.BranchLine : TrackTypeEnum.MainLine;
        }
    }

    public void Secondary() {
        if (Entity is StraightEntity straight) {
            straight.TrackAttribute = straight.TrackAttribute == TrackAttributeEnum.Normal ? TrackAttributeEnum.Hidden : TrackAttributeEnum.Normal;
        }
    }
}