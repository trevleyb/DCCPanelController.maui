using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : DrawingEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Aspect Ratio", "", 0, "Image")]
    private Aspect _aspectRatio = Aspect.AspectFit;

    [ObservableProperty] [property: Editable("Border Color", "", 0, "Image")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Border Radius", "", 0, "Image")]
    private int _borderRadius;

    [ObservableProperty] [property: Editable("Border Width", "", 0, "Image")]
    private int _borderWidth;

    [ObservableProperty] [property: Editable("Image", "", 0, "Select Image")]
    private string _image = string.Empty;

    [JsonConstructor]
    public ImageEntity() { }

    public ImageEntity(Panel panel) : this() => Parent = panel;

    public ImageEntity(ImageEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;

    [JsonIgnore] public override string EntityName => "Image";
    [JsonIgnore] public override string EntityDescription => "Selectable Image";
    [JsonIgnore] public override string EntityInformation => 
        "The Image Tile allows you to place an image on your canvas. This is either a file from the local device, or if you grant permission, the file can come from your Photo Library. "+ 
        "The image itself is stored inside the panel as a copy and so changed to the original will not be reflected unless you change the image in the properties. " +
        "An image can have opacity and can be scaled so you can place it below or above other objkects on your panel.";

    public override Entity Clone() => new ImageEntity(this);
}