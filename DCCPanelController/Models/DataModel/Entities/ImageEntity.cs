using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity, IRotationEntity {
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

    [ObservableProperty] [property: EditableOpacity("Opacity", group: "Image")]
    private double _opacity = 0.5;

    [JsonConstructor]
    public ImageEntity() { }

    public ImageEntity(Panel panel) : this() {
        Parent = panel;
    }

    public ImageEntity(ImageEntity entity) : base(entity) {
        BorderRadius = entity.BorderRadius;
        BorderWidth = entity.BorderWidth;
        BorderColor = entity.BorderColor;
        AspectRatio = entity.AspectRatio;
        Image = entity.Image;
    }

    public override string EntityName => "Image";

    public override Entity Clone() {
        return new ImageEntity(this);
    }
}