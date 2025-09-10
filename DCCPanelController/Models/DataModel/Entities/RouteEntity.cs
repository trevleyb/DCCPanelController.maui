using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RouteEntity : Entity, IEntityID, IInteractiveEntity {

    [ObservableProperty] [property: Editable("Route", "Select the route triggered by this button", 1, "General", EditorKind = EditorKinds.Route)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size", Order = 2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 1, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 2, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 3, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 4, "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("Indicator Color", "Default color of the Indicator", 8, "Colors")]
    private Color? _colorIndicator;

    [ObservableProperty] [property: Editable("Show Indicator", "Show the Button Indicator?", 7, "Colors")]
    private bool _showIndicator = true;

    
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