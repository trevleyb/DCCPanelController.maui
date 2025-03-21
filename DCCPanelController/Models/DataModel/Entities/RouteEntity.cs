using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IInteractiveEntity, IActionEntity {
    public override string Name => "Route";

    [ObservableProperty] [property: EditableButtonSize("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions")] 
    private ButtonActions _buttonPanelActions = [];
    
    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions")]
    private TurnoutActions _turnoutPanelActions = [];
    
    [ObservableProperty] private ButtonStateEnum _state  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public RouteEntity() {}
    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) {}
    public override Entity Clone() => new RouteEntity(this);
}