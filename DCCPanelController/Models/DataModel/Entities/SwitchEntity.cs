using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : Entity, IEntityID, IInteractiveEntity {
    [ObservableProperty] [property: EditableColor("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: EditableColor("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: EditableColor("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: EditableColor("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty]
    [property: EditableLightSwitch("DCC Light", "Select the ID of the light controlled by this button", 10, "Actions")]
    private string _id = string.Empty;

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Off;

    [ObservableProperty]
    [property: EditableEnum("Use Switch")]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;

    [JsonConstructor]
    public SwitchEntity() { }

    public SwitchEntity(Panel panel) : base(panel) { }
    public SwitchEntity(SwitchEntity entity) : base(entity) { }

    [JsonIgnore]
    public Light? Light => Parent?.Light(Id);

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Switch";

    public override Entity Clone() {
        return new SwitchEntity(this);
    }

    public override string ToString() {
        return Id;
    }
}