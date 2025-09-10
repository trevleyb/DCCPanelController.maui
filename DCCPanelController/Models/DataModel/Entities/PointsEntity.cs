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
    
    public PointsEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;

    public PointsEntity(PointsEntity entity) : base(entity) { }
    public override string EntityName => "Points";
    public override string EntityDescription => "Connection Points";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new PointsEntity(this);
    }
}