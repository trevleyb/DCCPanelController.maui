using CommunityToolkit.Maui.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class ButtonTile : Tile, ITileInteractive  {
    
    public ButtonTile(Entity entity, double gridSize) : base(entity, gridSize) { }

    private int lastColor = 0;
    private Color[] colors = [Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow];
    
    public Grid? TileContent { get; set => SetField(ref field, value); }
    public Color Color { get; set => SetField(ref field, value); }
    
    public override void CreateTile() {
        Color = colors[lastColor];
        TileContent = new Grid() {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
        TileContent.SetBinding(BackgroundColorProperty, new Binding(nameof(Color), source: this));
        Content = TileContent;
        Content.SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), source: this));
        Content.SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), source: this));
        Content.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), source: Entity));
    }

    public void Interact() {
        lastColor = (lastColor + 1) % colors.Length;
        Color = colors[lastColor];
    }
    
}