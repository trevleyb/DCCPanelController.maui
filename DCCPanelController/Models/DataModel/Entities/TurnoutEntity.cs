using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : Entity, IEntityID, IInteractiveEntity, ITrackEntity {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    [ObservableProperty] private Color? _trackColor = null;
    
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