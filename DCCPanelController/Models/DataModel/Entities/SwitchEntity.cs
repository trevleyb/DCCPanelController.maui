using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : Entity, IEntityID, IInteractiveEntity {
    
    [ObservableProperty]
    [property: Editable("Light", "Select the ID of the light controlled by this button", 1, "General", EditorKind = EditorKinds.Light)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size", Order=2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty]
    [property: Editable("Switch Style", Order=3)]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;
    
    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 1, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 2, "Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 3, "Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 4, "Colors")]
    private Color? _colorOffBorder;
    
    [ObservableProperty] [property: Editable("Unknown Color", "Override default 'Unknown' color", 5, Group="Colors")]
    private Color? _colorUnknown;

    [ObservableProperty] [property: Editable("Unknown Border Color", "Override default 'Unknown' border color", 6, Group="Colors")]
    private Color? _colorUnknownBorder;

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Off;

    [JsonConstructor]
    public SwitchEntity() { }

    public SwitchEntity(Panel panel) : base(panel) { }
    public SwitchEntity(SwitchEntity entity) : base(entity) { }

    [JsonIgnore]
    public Light? Light => Parent?.Light(Id);

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Switch";
    public override string EntityDescription => "On/Off Switch";
    public override string EntityInformation => 
        "This button turns on or off a __switch__. ";

    public override Entity Clone() {
        return new SwitchEntity(this);
    }

    public override string ToString() {
        return Id;
    }
}