using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using ExCSS;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PointsEntity : Entity, IDrawingEntity {
    [JsonConstructor]
    public PointsEntity() { }

    [ObservableProperty] [property: Editable("Name")] private string? _name;
    [ObservableProperty] [property: Editable("Text", Description = "Enter some text dufus" ,EditorKind = "text")] private string? text;
    [ObservableProperty] [property: Editable("Toggle", EditorKind = "toggle")] private bool toggle;
    [ObservableProperty] [property: Editable("Color", Order = 0)] private Color _color;
    
    public PointsEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;

    public PointsEntity(PointsEntity entity) : base(entity) { }
    public override string EntityName => "Points";
    public override string EntityDescription => "Connection Points";
    public override Entity Clone() {
        return new PointsEntity(this);
    }
}