using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : Entity, IEntityID, IInteractiveEntity {
    
    [ObservableProperty] [property: Editable("Button Size")]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty]
    [property: Editable("DCC Light", "Select the ID of the light controlled by this button", 10, "Actions", EditorKind = EditorKinds.Light)]
    private string _id = string.Empty;

    [ObservableProperty]
    [property: Editable("Switch Style")]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;

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
    
    public override Entity Clone() {
        return new SwitchEntity(this);
    }

    public override string ToString() {
        return Id;
    }
}