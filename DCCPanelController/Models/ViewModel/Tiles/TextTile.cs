using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TextTile : Tile {
    public TextTile(TextEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TextEntity.Label));
        VisualProperties.Add(nameof(entity.Label));
        VisualProperties.Add(nameof(entity.FontSize));
        VisualProperties.Add(nameof(entity.TextColor));
        VisualProperties.Add(nameof(entity.HorizontalJustification));
        VisualProperties.Add(nameof(entity.VerticalJustification));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is TextEntity entity && !string.IsNullOrEmpty(entity.Label)) {

            Microsoft.Maui.Controls.View view;
            var label = new Label {
                Text = entity.Label,
                FontSize = entity.FontSize,
                HorizontalTextAlignment = entity.HorizontalJustification,
                VerticalTextAlignment = entity.VerticalJustification,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TextColor = entity.TextColor,
                BackgroundColor = BackgroundColor,
                ZIndex = entity.Layer,
                RotationX = entity.Rotation,
                LineBreakMode = LineBreakMode.TailTruncation,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight
            };

            if (entity.BorderWidth > 0) {
                view = new Border {
                    Content = label,
                    InputTransparent = true,
                    RotationX = entity.Rotation,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    WidthRequest = TileWidth,
                    HeightRequest = TileHeight,
                    StrokeThickness = entity.BorderWidth,
                    Stroke = entity.BorderColor,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(entity.BorderRadius) }
                };
            } else view = label;
            view.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return view;
        } 
        return CreateSymbol();
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("text").AsImage();
    }

}