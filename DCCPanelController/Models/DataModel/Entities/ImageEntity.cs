using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity {
    public override string Name => "Image";
    
    [ObservableProperty][property: EditableInt("Border Radius", group: "Image")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: EditableInt("Border Width", group: "Image")]
    private int _borderWidth = 0;
    
    [ObservableProperty] [property: EditableColor("Border Color",  group: "Image")]
    private Color _borderColor = Colors.Transparent;
    
    [ObservableProperty] [property: EditableAspectRatio ("Aspect Ratio",group: "Image")]
    private Aspect _aspectRatio = Aspect.AspectFit;
    
    [ObservableProperty] [property: EditableImage("Image", group: "Image")]
    private string _image = string.Empty;

    [JsonConstructor]
    public ImageEntity() {}
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
    public override Entity Clone() => new ImageEntity(this);
}
 