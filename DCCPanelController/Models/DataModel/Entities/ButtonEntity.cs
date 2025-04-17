using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : Entity, IEntityID, IInteractiveEntity, IActionEntity {
    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions")]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: EditableButtonSize("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableID("Button ID")]
    private string _id = string.Empty;

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions")]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    public ButtonEntity() { }

    public ButtonEntity(Panel panel) : base(panel) { }
    public ButtonEntity(ButtonEntity entity) : base(entity) { }
    public override string EntityName => "Button";

    public string GenerateID() {
        return EntityID.NextButtonID(Parent?.GetAllEntitiesByType<ButtonEntity>() ?? []);
    }

    public override Entity Clone() {
        return new ButtonEntity(this);
    }
}