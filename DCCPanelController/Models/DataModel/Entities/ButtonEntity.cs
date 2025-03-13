using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : Entity, IEntityID, IInteractiveEntity, IButtonEntity {
    public override string Name => "Button";

    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private ButtonStateEnum _state  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public ButtonEntity() {}
    public ButtonEntity(Panel panel) : base(panel) { }
    public ButtonEntity(ButtonEntity entity) : base(entity) {}
    public override Entity Clone() => new ButtonEntity(this);
    public string GenerateID() => EntityID.NextButtonID(Parent?.GetAllEntitiesByType<ButtonEntity>() ??[]);
}