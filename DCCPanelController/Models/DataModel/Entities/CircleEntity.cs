using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : DrawingEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background","", 0, "Circle")]
    private Color _backgroundColor = Colors.Gray;

    [ObservableProperty] [property: Editable("Border", "",0, "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty]
    [property: Editable("Border Width", "", 0, "Circle")]
    private int _borderWidth = 1;
    
    [JsonConstructor]
    public CircleEntity() { }

    public CircleEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;
    
    public CircleEntity(CircleEntity entity) : base(entity) { }

    public override string EntityName => "Circle";
    public override string EntityDescription => "Adjustable Circle";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new CircleEntity(this);
    }
}