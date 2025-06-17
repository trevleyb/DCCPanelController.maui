using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IInteractiveEntity {
    [ObservableProperty] [property: EditableEnum("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableRoute("Route", "", 0)]
    private string _routeID = string.Empty;

    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    [JsonConstructor]
    public RouteEntity() { }

    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) { }
    public override string EntityName => "Route";

    [JsonIgnore]
    public Route? Route => Parent?.Route(RouteID);

    public override Entity Clone() {
        return new RouteEntity(this);
    }

    public override string ToString() {
        return RouteID;
    }
}