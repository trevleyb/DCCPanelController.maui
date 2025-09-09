using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.DynamicProperties;
using ExCSS;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Entities;
  
public partial class CompassEntity : Entity {
    
    public CompassEntity() { }

    public CompassEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;
    
    public CompassEntity(CompassEntity entity) : base(entity) { }
    public override string EntityName => "Compass";
    public override string EntityDescription => "Directional Compass";
    
    public override Entity Clone() {
        return new CompassEntity(this);
    }

}