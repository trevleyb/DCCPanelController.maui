namespace DCCPanelController.Tracks.StyleManager;

public record StyleImageRotation(string ImageSource, int TrackRotation, int ImageRotation);

public class StyleTrackImages {
    private readonly List<StyleTrackImage> _supportedImages = [];

    public StyleImageRotation GetTrackImageSourceAndRotation(TrackStyleImageEnum imageEnumStyle, int trackRotation) {
        // Maximum number of attempts to find the correct image by incrementing the rotation
        const int maxAttempts = 8; // This allows for searching up to a full 360 degrees
        const int rotationIncrement = 45;

        for (var attempt = 0; attempt < maxAttempts; attempt++) {
            var currentRotation = (trackRotation + attempt * rotationIncrement) % 360;

            foreach (var supportedImage in _supportedImages) {
                if (supportedImage.ImageEnumStyle == imageEnumStyle) {
                    foreach (var rotation in supportedImage.Rotations) {
                        if (rotation.TrackRotation == currentRotation) {
                            return new StyleImageRotation(supportedImage.ImageSource, rotation.TrackRotation, rotation.ImageRotation);
                        }
                    }
                }
            }
        }

        Console.WriteLine($"Unable to find image for {imageEnumStyle} at {trackRotation}");
        return new StyleImageRotation("Unknown", 0, 0);
    }

    private void AddStyleTrackImage(StyleTrackImage styleTrackImage) {
        _supportedImages.Add(styleTrackImage);
    }

    public void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        var builder = StyleTrackImage.Create(trackType, imageSource);

        if (rotations.Length <= 0) builder.AddDefaultRotations();
        else builder.AddRotations(rotations);

        AddStyleTrackImage(builder.Build());
    }

    public void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        var builder = StyleTrackImage.Create(trackType, imageSource);
        builder.AddRotations(rotations);
        AddStyleTrackImage(builder.Build());
    }

    public StyleTrackImages Clone() {
        var styleImagesCopy = new StyleTrackImages();

        foreach (var image in _supportedImages) {
            var imageCopy = image.Clone();
            styleImagesCopy.AddStyleTrackImage(imageCopy);
        }

        return styleImagesCopy;
    }
}