using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ImageEntity : Entity, IDrawingEntity {
    public override string Name => "Image";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth = 0;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Aspect _aspectRatio = Aspect.AspectFit;
    [ObservableProperty] private string _image = string.Empty;

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
 