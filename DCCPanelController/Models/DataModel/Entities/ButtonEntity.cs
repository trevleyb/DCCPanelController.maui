using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : Entity, IEntityID, IInteractiveEntity, IActionEntity {

    [ObservableProperty] [property: EditableID("Button ID","",0)]
    private string _id = string.Empty;

    [ObservableProperty] [property: EditableEnum("Button Size","",1)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions")]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions")]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    public ButtonEntity() { }

    public ButtonEntity(Panel panel) : base(panel) { }

    public ButtonEntity(ButtonEntity entity) : base(entity) {
        ButtonSize = entity.ButtonSize;
        State = entity.State;
    }
    public override string EntityName => "Button";

    public string GenerateID() {
        return EntityID.NextButtonID(Parent?.GetAllEntitiesByType<ButtonEntity>() ?? []);
    }

    public override Entity Clone() {
        return new ButtonEntity(this);
    }
}