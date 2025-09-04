using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TextEntity.Label));
        VisualProperties.Add(nameof(entity.Label));
        VisualProperties.Add(nameof(entity.FontSize));
        VisualProperties.Add(nameof(entity.FontStyle));
        VisualProperties.Add(nameof(entity.TextColor));
        VisualProperties.Add(nameof(entity.HorizontalJustification));
        VisualProperties.Add(nameof(entity.VerticalJustification));
        VisualProperties.Add(nameof(entity.BackgroundColor));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TextEntity entity && !string.IsNullOrEmpty(entity.Label)) {
            var label = new Label {
                HorizontalTextAlignment = EnumHelpers.ConvertHorizontalAlignmentToText(entity.HorizontalJustification),
                VerticalTextAlignment = EnumHelpers.ConvertVerticalAlignmentToText(entity.VerticalJustification),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                ZIndex = entity.Layer,
                LineBreakMode = LineBreakMode.TailTruncation,
                Rotation = entity.Rotation,
                FontSize = entity.FontSize,
                FontAttributes = EnumHelpers.ConvertFontStyle(entity.FontStyle),
                TextColor = entity.TextColor,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                Text = entity.Label
            };
            return label;
        }
        return CreateSymbol(); // Fallback on error
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("text").AsImage();
    }
}