using DCCPanelController.Components.TrackPieces.ImageManager;
using SvgImage = DCCPanelController.Components.TrackPieces.ImageManager.SvgImage;

namespace DCCPanelController.Components.TrackPieces.Base;

public struct TrackImage(SvgImage imageSource, int rotation) {
    public TrackImage(string imageName, int rotation) : this(SvgImages.Create(imageName), rotation) { }
    public SvgImage ImageSource { get; init; } = imageSource; // Path to the image file
    public int Rotation { get; init; } = rotation;
}