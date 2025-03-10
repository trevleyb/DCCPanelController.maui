using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CornerButtonEntity : ToggleButtonEntity, IEntityID, IInteractiveEntity, IButtonEntity {
    public override string Name => "CornerButton";
    
    [JsonConstructor]
    public CornerButtonEntity() {}
    public CornerButtonEntity(Panel panel) : base(panel) { }
    public CornerButtonEntity(CornerButtonEntity entity) : base(entity) {}
    public override Entity Clone() => new CornerButtonEntity(this);

}