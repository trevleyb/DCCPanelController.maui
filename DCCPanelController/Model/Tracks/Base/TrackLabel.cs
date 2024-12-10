using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackLabelBase : TrackBase {
    [ObservableProperty] 
    [property: EditableStringProperty(Name = "Circle Label", Description = "Label to display in the Circle")]
    private string _circlelabel = string.Empty;

    [JsonIgnore]
    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Normal, TrackRotation);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            ImageRotation = trackInfo.ImageRotation;
            TrackRotation = trackInfo.TrackRotation;
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackStyleImage.Normal, Parent?.Defaults);
            style = SvgStyles.AddTextToStyle(style, Circlelabel);
            return imageInfo.ApplyStyle(style);
        }
    }

    [JsonIgnore]
    protected override SvgImage SymbolImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Symbol, 0);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackStyleImage.Normal, Parent?.Defaults);
            return imageInfo.ApplyStyle(style);
        }
    }
}