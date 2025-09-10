using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LineEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Line Color", "",0, "Line")]
    private Color _lineColor = Colors.Black;
    
    [ObservableProperty] [property: Editable("Line Width", "",0, "Line")]
    private int _lineWidth = 3;

    [JsonConstructor]
    public LineEntity() { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public LineEntity(Panel panel) : this() {
        Parent = panel;
    }

    public LineEntity(LineEntity entity) : base(entity) { }

    public override string EntityName => "Line";
    public override string EntityDescription => "Straight Line";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new LineEntity(this);
    }
}