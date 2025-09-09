using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Aspect Ratio", "",0, "Image")]
    private Aspect _aspectRatio = Aspect.AspectFit;

    [ObservableProperty] [property: Editable("Border Color", "",0, "Image")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Border Radius", "",0,"Image")]
    private int _borderRadius;

    [ObservableProperty] [property: Editable("Border Width", "",0,"Image")]
    private int _borderWidth;

    [ObservableProperty] [property: Editable("Image", "",0, "Select Image")]
    private string _image = string.Empty;
 
    [JsonConstructor]
    public ImageEntity() { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public ImageEntity(Panel panel) : this() {
        Parent = panel;
    }

    public ImageEntity(ImageEntity entity) : base(entity) { }

    public override string EntityName => "Image";
    public override string EntityDescription => "Selectable Image";
    
    public override Entity Clone() {
        return new ImageEntity(this);
    }
}