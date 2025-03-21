using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.DynamicProperties;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IEntityID, IInteractiveEntity, ITrackEntity {
    [ObservableProperty] [property: EditableID("ID", "", 0, group: "Turnout")] 
    private string _id = string.Empty;
    
    [ObservableProperty] [property: EditableString("DCC Address", "", 0, group: "Turnout")]
    private string _address = string.Empty;
    
    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions", ActionsContext.Turnout)] 
    private ButtonActions _buttonPanelActions = [];
    
    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private TurnoutActions _turnoutPanelActions = [];
    
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    
    [JsonConstructor]
    protected TurnoutEntity() {}
    protected TurnoutEntity(Panel panel) : base(panel) { }
    protected TurnoutEntity(TurnoutEntity entity) : base(entity) {
        Address = string.Empty;
        ButtonPanelActions = new ButtonActions(entity.ButtonPanelActions);
        TurnoutPanelActions = new TurnoutActions(entity.TurnoutPanelActions);
        State = TurnoutStateEnum.Unknown;
        TrackColor = entity.TrackColor;
    }
    public string GenerateID() {
        var entities = Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? new List<TurnoutEntity>();
        return EntityID.NextTurnoutID(entities);
    }
}