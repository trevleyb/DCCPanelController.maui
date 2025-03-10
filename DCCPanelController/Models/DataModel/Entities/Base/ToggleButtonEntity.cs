using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class ToggleButtonEntity : Entity, IEntityID, IInteractiveEntity, IButtonEntity {
    public override string Name => "Toggle Button";
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private ButtonStateEnum _buttonState  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public ToggleButtonEntity() { }
    public ToggleButtonEntity(Panel panel) : base(panel) { }
    public ToggleButtonEntity(ToggleButtonEntity entity) : base(entity) {
        ButtonActions = new Actions<ButtonStateEnum>(entity.ButtonActions);
        TurnoutActions = new Actions<TurnoutStateEnum>(entity.TurnoutActions);
        ButtonState = ButtonStateEnum.Unknown;
    }
    public string GenerateID() => EntityID.NextButtonID(Parent?.GetAllEntitiesByType<ToggleButtonEntity>() ??[]);
}