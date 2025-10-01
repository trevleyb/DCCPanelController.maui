namespace DCCPanelController.Models.ViewModel.TileCache;

internal readonly record struct TileRenderKey(
    string Asset,     // e.g., SvgImage.Filename (full resource name/path)
    int RotationDeg,  // final degrees you’ll draw with (normalize to 0..359)
    int PixelWidth,   // device-pixel width of the surface
    int PixelHeight,  // device-pixel height of the surface
    string StyleHash, // hash of the applied SvgStyle (colors/visibility/etc.)
    string? Flags     // "Path"/"Occ"/null or any other visual toggles that change pixels
);

