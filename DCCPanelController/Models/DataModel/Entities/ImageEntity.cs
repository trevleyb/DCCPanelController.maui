using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: EditableEnum("Aspect Ratio", group: "Image")]
    private Aspect _aspectRatio = Aspect.AspectFit;

    [ObservableProperty] [property: EditableColor("Border Color", group: "Image")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt("Border Radius", group: "Image")]
    private int _borderRadius;

    [ObservableProperty] [property: EditableInt("Border Width", group: "Image")]
    private int _borderWidth;

    [ObservableProperty] [property: EditableImage("Image", group: "Image")]
    private string _image = string.Empty;

    [JsonConstructor]
    public ImageEntity() { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public ImageEntity(Panel panel) : this() {
        Parent = panel;
    }

    public ImageEntity(ImageEntity entity) : base(entity) { }

    public override string EntityName => "Image";

    public override Entity Clone() {
        return new ImageEntity(this);
    }
}