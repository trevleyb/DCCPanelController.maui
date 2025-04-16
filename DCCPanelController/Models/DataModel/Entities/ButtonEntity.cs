using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ButtonEntity : Entity, IEntityID, IInteractiveEntity, IActionEntity {
    public override string EntityName => "Button";

    [ObservableProperty] [property: EditableID("Button ID")]
    private string _id = string.Empty;
    
    [ObservableProperty] [property: EditableButtonSize("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, group: "Actions", ActionsContext.Button)] 
    private ButtonActions _buttonPanelActions = [];
    
    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions", ActionsContext.Button)]
    private TurnoutActions _turnoutPanelActions = [];
    
    [ObservableProperty] private ButtonStateEnum _state  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public ButtonEntity() {}
    public ButtonEntity(Panel panel) : base(panel) { }
    public ButtonEntity(ButtonEntity entity) : base(entity) {}
    public override Entity Clone() => new ButtonEntity(this);
    public string GenerateID() => EntityID.NextButtonID(Parent?.GetAllEntitiesByType<ButtonEntity>() ??[]);
}