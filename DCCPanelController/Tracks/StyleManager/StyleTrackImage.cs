namespace DCCPanelController.Tracks.StyleManager;

public class StyleTrackImage {
    private StyleTrackImage(Builder builder) {
        ImageSource = builder.ImageSource;
        ImageEnumStyle = builder.ImageEnum;
        Rotations = builder.Rotations;
    }

    public string ImageSource { get; private set; }
    public TrackStyleImageEnum ImageEnumStyle { get; private set; }
    public List<Rotation> Rotations { get; private set; }

    public static Builder Create(TrackStyleImageEnum trackType, string imageSource) {
        return new Builder(trackType, imageSource);
    }

    public StyleTrackImage Clone() {
        return (StyleTrackImage)MemberwiseClone();
    }

    public class Rotation {
        public int TrackRotation { get; set; }
        public int ImageRotation { get; set; }
    }

    public class Builder(TrackStyleImageEnum track, string imageSource) {
        public string ImageSource { get; } = imageSource ?? throw new ArgumentNullException(nameof(imageSource));
        public TrackStyleImageEnum ImageEnum { get; } = track;
        public List<Rotation> Rotations { get; } = new();

        public Builder AddRotation(int trackRotation, int imageRotation) {
            Rotations.Add(new Rotation { TrackRotation = trackRotation, ImageRotation = imageRotation });
            return this;
        }

        public Builder AddRotations(List<Rotation> rotations) {
            foreach (var rotation in rotations) {
                AddRotation(rotation.TrackRotation, rotation.ImageRotation);
            }

            return this;
        }

        public Builder AddRotations(params (int TrackRotation, int ImageRotation)[] rotations) {
            foreach (var rotation in rotations) {
                AddRotation(rotation.TrackRotation, rotation.ImageRotation);
            }

            return this;
        }

        public Builder AddDefaultRotations() {
            return AddRotations((0, 0), (90, 0), (180, 0), (270, 0));
        }

        public StyleTrackImage Build() {
            return new StyleTrackImage(this);
        }
    }
}