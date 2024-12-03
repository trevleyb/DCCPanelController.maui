namespace DCCPanelController.Tracks.StyleManager;

using System;
using System.Collections.Generic;

public class StyleTrackImage {
    public string ImageSource { get; private set; }
    public TrackStyleImage Image { get; private set; }
    public List<Rotation> Rotations { get; private set; }

    private StyleTrackImage(Builder builder) {
        ImageSource = builder.ImageSource;
        Image = builder.Image;
        Rotations = builder.Rotations;
    }

    public static Builder Create(TrackStyleImage trackType, string imageSource) => new Builder(trackType, imageSource);

    public class Rotation {
        public int TrackRotation { get; set; }
        public int ImageRotation { get; set; }
    }

    public class Builder(TrackStyleImage track, string imageSource) {
        public string ImageSource { get; } = imageSource ?? throw new ArgumentNullException(nameof(imageSource));
        public TrackStyleImage Image { get; } = track;
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
            return AddRotations((0,0),(90,0),(180,0),(270,0));
        }

        public StyleTrackImage Build() {
            return new StyleTrackImage(this);
        }
    }
}