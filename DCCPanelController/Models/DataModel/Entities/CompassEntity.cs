using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CompassEntity : Entity, IDrawingEntity {
    public CompassEntity() { }

    public CompassEntity(Panel panel) : this() => Parent = panel;

    public CompassEntity(CompassEntity entity) : base(entity) { }

    [ObservableProperty] [property: Editable("Background", "", 1, "Compass")]
    private Color _backgroundColor = Colors.White;

    [ObservableProperty] [property: Editable("Border", "", 2, "Compass")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: Editable("Border Width", "", 3, Group = "Compass")]
    private int _borderWidth = 1;

    [ObservableProperty] [property: Editable("Directions", "", 10, "Compass")]
    private Color _directionsColor = Colors.Black;

    [ObservableProperty] [property: Editable("Indicators", "", 11, "Compass")]
    private Color _indicatorsColor = Colors.Black;
    
    [JsonIgnore] protected override int RotationFactor => 45;
    [JsonIgnore] public override string EntityName => "Compass";
    [JsonIgnore] public override string EntityDescription => "Directional Compass";
    [JsonIgnore] public override string EntityInformation =>
        "The Compare is a simple compass which shows the direction of travel";
    
    public override Entity Clone() => new CompassEntity(this);
}