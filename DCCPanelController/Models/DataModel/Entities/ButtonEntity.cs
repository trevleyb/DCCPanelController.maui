using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : ToggleButtonEntity, IEntityID, IInteractiveEntity, IButtonEntity {
    public override string Name => "Button";

    [JsonConstructor]
    public ButtonEntity() {}
    public ButtonEntity(Panel panel) : base(panel) { }
    public ButtonEntity(ButtonEntity entity) : base(entity) {}
    public override Entity Clone() => new ButtonEntity(this);
}