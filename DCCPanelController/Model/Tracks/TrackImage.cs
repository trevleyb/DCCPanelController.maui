using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : TrackPieceBase(parent), ITrackSymbol, ITrackPiece {
    public TrackImage() : this(null) { }
    
    [EditableImageProperty(Name = "Image", Description = "Image to display")]
    public string TrackImageSource { get; set; } = "";
    
    [ObservableProperty] [property: EditableIntProperty(Name = "Width", Description = "Text Grid Width", Group = "Attributes")]
    private int _imageWidth = 2;

    [ObservableProperty] [property: EditableIntProperty(Name = "Height", Description = "Text Grid Height", Group = "Attributes")]
    private int _imageHeight = 2;
    
    public ImageSource? Image => ImageHelper.ImageFromBase64(TrackImageSource);
    
    protected override void Setup() {
        Layer = 2;
        Name = "ImageStyle";
        RotationIncrement = 90;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
    }
    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackImage>(parent);
    }

    //public static string SerializeImage(ImageSource? imageSource) {
    //    var imageBytes = Convert.FromBase64String(Base64ImageProvider.Base64EncodedImage);
    //    MemoryStream imageDecodeStream = new(imageBytes);
    //    base64DecodedImage.Source = ImageSource.FromStream(() => imageDecodeStream);
    //}

    public static ImageSource? DeserializeImage(string base64String) {
        var imageBytes = Convert.FromBase64String(base64String);
        MemoryStream imageDecodeStream = new(imageBytes);
        return ImageSource.FromStream(() => imageDecodeStream);    
    }
}