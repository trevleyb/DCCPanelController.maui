using DCCPanelController.Tracks.ImageManager;
using SvgImage = DCCPanelController.Tracks.ImageManager.SvgImage;

namespace DCCPanelController.Tracks.Base;

public struct TrackImage(SvgImage imageSource, int rotation) {
    public TrackImage(string imageName, int rotation) : this(SvgImages.Create(imageName), rotation) { }
    public SvgImage ImageSource { get; init; } = imageSource; // Path to the image file
    public int Rotation { get; init; } = rotation;
}