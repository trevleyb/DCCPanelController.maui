using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IEntityID, IInteractiveEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("ID", EditableType.Id)] 
    private string _id = string.Empty;
    
    [ObservableProperty] [property: Editable("DCC Address", EditableType.String)]
    private string _address = string.Empty;
    
    [ObservableProperty] [property: Editable("Button Actions", EditableType.ButtonActions, 0, "Actions", ActionsContext.Turnout)] 
    private Actions<ButtonStateEnum> _buttonActions = [];
    
    [ObservableProperty] [property: Editable("Turnout Actions", EditableType.TurnoutActions, 0, "Actions", ActionsContext.Turnout)]
    private Actions<TurnoutStateEnum> _turnoutActions = [];
    
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    
    [JsonConstructor]
    protected TurnoutEntity() {}
    protected TurnoutEntity(Panel panel) : base(panel) { }
    protected TurnoutEntity(TurnoutEntity entity) : base(entity) {
        Address = string.Empty;
        ButtonActions = new Actions<ButtonStateEnum>(entity.ButtonActions);
        TurnoutActions = new Actions<TurnoutStateEnum>(entity.TurnoutActions);
        State = TurnoutStateEnum.Unknown;
        TrackColor = entity.TrackColor;
    }
    public string GenerateID() {
        var entities = Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? new List<TurnoutEntity>();
        return EntityID.NextTurnoutID(entities);
    }
}