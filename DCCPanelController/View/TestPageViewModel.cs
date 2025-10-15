using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.TileSelectors;

namespace DCCPanelController.View;

public partial class TestPageViewModel : ObservableObject {

    public Palette Palette { get; init; } = PaletteCache.GetDefaultPalette();
    public Palette SidePalette { get; init; } = PaletteCache.GetPalette("Side");
    public Palette BottomPalette { get; init; } = PaletteCache.GetPalette("Bottom");

}