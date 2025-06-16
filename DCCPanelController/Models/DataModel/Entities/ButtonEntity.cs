using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : Entity, IEntityID, IInteractiveEntity, IActionEntity {

    [ObservableProperty] [property: EditableID("Button Name","",0)]
    private string _id = string.Empty;

    [ObservableProperty] [property: EditableEnum("Button Size","",1)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions")]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions")]
    private TurnoutActions _turnoutPanelActions = [];

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [JsonConstructor]
    public ButtonEntity() {
        Id = NextID;
    }

    public ButtonEntity(Panel panel) : base(panel) { }
    public ButtonEntity(ButtonEntity entity) : base(entity) {
        ButtonSize = entity.ButtonSize;
        State = entity.State;
    }
    public override string EntityName => "Button";

    [JsonIgnore]
    public List<IEntityID> AllIDs => new List<IEntityID>(Parent?.GetAllEntitiesByType<ButtonEntity>() ?? []) ?? [];
    public string NextID => EntityID.GenerateNextID(Parent?.GetAllEntitiesByType<ButtonEntity>() ?? [],"Button");

    public override Entity Clone() {
        return new ButtonEntity(this);
    }

    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)this.ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)this.TurnoutPanelActions.Clone();
    }
}