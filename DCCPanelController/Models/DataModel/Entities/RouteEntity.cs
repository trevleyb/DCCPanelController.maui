using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Views.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IEntityID, IInteractiveEntity {
    [ObservableProperty] [property: EditableEnum("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableRoute("Route", "Select the route triggered by this button", 10, "Actions")]
    private string _id = string.Empty;

    [ObservableProperty] [property: EditableColor("Indicator Color", "Default color of the Indicator", 5, "Colors")]
    private Color? _routeIndicator;

    [ObservableProperty] [property: EditableColor("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: EditableColor("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: EditableColor("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: EditableColor("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;

    
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    [JsonConstructor]
    public RouteEntity() { }

    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) { }
    public override string EntityName => "Route";

    [JsonIgnore]
    public Route? Route => Parent?.Route(Id);

    [JsonIgnore] protected override int RotationFactor => 90;
    
    public override Entity Clone() {
        return new RouteEntity(this);
    }

    public override string ToString() {
        return Id;
    }
}