using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Actions;
using DCCPanelController.View.DynamicProperties;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IInteractiveEntity {
    public override string EntityName => "Route";

    [ObservableProperty] [property: EditableButtonSize("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableRoute("Route")]
    private string _dccAddress = string.Empty;
    
    [ObservableProperty] private ButtonStateEnum _state  = ButtonStateEnum.Unknown;
    
    [JsonConstructor]
    public RouteEntity() {}
    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) {}
    public override Entity Clone() => new RouteEntity(this);
    public override string ToString() => DccAddress;
}