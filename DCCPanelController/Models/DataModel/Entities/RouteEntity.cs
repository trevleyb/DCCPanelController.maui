using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IInteractiveEntity, IButtonEntity {
    public override string Name => "Route";

    [ObservableProperty] [property: Editable("Button Size", EditableType.ButtonSize)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Button Actions", EditableType.ButtonActions, 10, "Actions", ActionsContext.Button)] 
    private ButtonActions _buttonPanelActions = [];
    
    [ObservableProperty] [property: Editable("Turnout Actions", EditableType.TurnoutActions, 10, "Actions", ActionsContext.Button)]
    private TurnoutActions _turnoutPanelActions = [];
    
    [ObservableProperty] private ButtonStateEnum _state  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public RouteEntity() {}
    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) {}
    public override Entity Clone() => new RouteEntity(this);
}