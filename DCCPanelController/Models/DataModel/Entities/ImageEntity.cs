using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity {
    public override string Name => "Image";
    
    [ObservableProperty][property: Editable("Border Radius", EditableType.Integer, group: "Image")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: Editable("Border Width", EditableType.Integer, group: "Image")]
    private int _borderWidth = 0;
    
    [ObservableProperty] [property: Editable("Border Color", EditableType.Color, group: "Image")]
    private Color _borderColor = Colors.Transparent;
    
    [ObservableProperty] [property: Editable("Aspect Ratio", EditableType.AspectRatio, group: "Image")]
    private Aspect _aspectRatio = Aspect.AspectFit;
    
    [ObservableProperty] [property: Editable("Image", EditableType.Image)]
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
 