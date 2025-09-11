using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : ButtonEntity, IEntityID, IInteractiveEntity {

    [ObservableProperty] [property: Editable("Route", "Select the route triggered by this button", 1, "General", EditorKind = EditorKinds.Route)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size", Order = 2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;
    
    [ObservableProperty] [property: Editable("Indicator Color", "Default color of the Indicator", 11, "Colors")]
    private Color? _colorIndicator;

    [ObservableProperty] [property: Editable("Show Indicator", "Show the Button Indicator?", 10, "Colors")]
    private bool _showIndicator = true;

    
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    [JsonConstructor]
    public RouteEntity() { }

    public RouteEntity(Panel panel) : base(panel) { }
    public RouteEntity(RouteEntity entity) : base(entity) { }
    public override string EntityName => "Route";
    public override string EntityDescription => "Route Trigger Button";

    public override string EntityInformation =>
        "This button sends the specified __route__ command to the controller to set a route. " +
        "If the route is set, this should trigger items such as __turnouts__ which should be reflected " +
        "on the control panel. ";

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