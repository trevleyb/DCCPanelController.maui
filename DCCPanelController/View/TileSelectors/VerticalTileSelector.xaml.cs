using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View;

public partial class VerticalTileSelector {
    public VerticalTileSelector() {
        InitializeComponent();
        BindingContext = new VerticalTileSelectorViewModel();
    }

    /// <summary>
    /// Capture the Symbol for use on the Control Surface
    /// </summary>
    private void SymbolDragStarting(object? sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer { BindingContext: Tile { } tile }) {
            if (e.Data.Properties is { } properties) {
                properties.Add("Tile", tile);
                properties.Add("Source", "Symbol");
            }
        }
    }
}