using AVFoundation;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using ExCSS;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(entity.HorizontalJustification));
        VisualProperties.Add(nameof(entity.VerticalJustification));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TextEntity entity) {
            if (string.IsNullOrEmpty(entity.Label)) entity.Label = "T";
            var label = new Label {
                HorizontalTextAlignment = EnumHelpers.ConvertHorizontalAlignmentToText(entity.HorizontalJustification),
                VerticalTextAlignment = EnumHelpers.ConvertVerticalAlignmentToText(entity.VerticalJustification),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                LineBreakMode = LineBreakMode.TailTruncation,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                FontFamily = EnumHelpers.ConvertFontStyle(entity.FontStyle),
            };
            label.SetBinding(Label.BackgroundColorProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, source: entity));
            label.SetBinding(Label.FontSizeProperty, new Binding(nameof(entity.FontSize), BindingMode.OneWay, source: entity));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(entity.TextColor), BindingMode.OneWay, source: entity));
            label.SetBinding(Label.RotationProperty, new Binding(nameof(entity.Rotation), BindingMode.OneWay, source: entity));
            label.SetBinding(Label.TextProperty, new Binding(nameof(entity.Label), BindingMode.OneWay, source: entity));
            label.SetBinding(Label.ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            return label;
        }
        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}