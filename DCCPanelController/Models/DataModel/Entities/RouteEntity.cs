using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IEntityID, IInteractiveEntity {
    [ObservableProperty] [property: Editable("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Route", "Select the route triggered by this button", 10, "General", EditorKind = EditorKinds.Route)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Indicator Color", "Default color of the Indicator", 5, "Colors")]
    private Color? _routeIndicator;

    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;
    
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    [JsonConstructor]
    public RouteEntity() { }

    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) { }
    public override string EntityName => "Route";
    public override string EntityDescription => "Route Trigger Button";
    
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