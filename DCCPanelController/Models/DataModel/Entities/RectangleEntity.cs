using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class RectangleEntity : Entity, IDrawingEntity {
    public override string Name => "Rectangle";
    
    [ObservableProperty][property: EditableInt("Border Width", group: "Rectangle")] 
    private int _borderWidth  = 1;

    [ObservableProperty] [property: EditableInt("Border Radius", group: "Rectangle")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: EditableColor("Background Color", group: "Rectangle")]
    private Color _backgroundColor = Colors.Transparent;
    
    [ObservableProperty] [property: EditableColor("Border Color", group: "Rectangle")]
    private Color _borderColor = Colors.Black;
   
    [ObservableProperty][property: EditableOpacity("Opacity", group: "Rectangle")] 
    private double _opacity = 1;
    
    [JsonConstructor]
    public RectangleEntity() {}
    public RectangleEntity(Panel panel) : this() {
        Parent = panel;
    }
    public RectangleEntity(RectangleEntity entity) : base(entity) {
        BackgroundColor = entity.BackgroundColor;
        BorderColor = entity.BorderColor;
        BorderWidth = entity.BorderWidth;
        Opacity = entity.Opacity;
    }
    public override Entity Clone() => new RectangleEntity(this);
}