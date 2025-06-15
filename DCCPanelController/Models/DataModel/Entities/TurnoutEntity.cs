using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IInteractiveEntity, ITrackEntity, IActionEntity {
    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private ButtonActions _buttonPanelActions = [];

    //[ObservableProperty] [property: EditableID("ID", "", 0, "Turnout")]
    //private string _id = string.Empty;

    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

    [ObservableProperty] [property: EditableTurnout("Turnout", "", 0, "Turnout")]
    private string _turnoutID = string.Empty;

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    protected TurnoutEntity() { }

    protected TurnoutEntity(Panel panel) : base(panel) { }

    protected TurnoutEntity(TurnoutEntity entity) : base(entity) {
        TurnoutID = string.Empty;
        ButtonPanelActions = new ButtonActions(entity.ButtonPanelActions);
        TurnoutPanelActions = new TurnoutActions(entity.TurnoutPanelActions);
        State = TurnoutStateEnum.Unknown;
        TrackColor = entity.TrackColor;
        RotationFactor = 90;
    }

    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(TurnoutID);

    public override string ToString() {
        return TurnoutID;
    }
}