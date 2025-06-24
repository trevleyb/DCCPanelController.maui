using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : Entity,  IInteractiveEntity {

    [ObservableProperty] 
    [property: EditableLightSwitch("Light", "Select the ID of the light controlled by this button", 10, "Actions")]
    private string _switchID = string.Empty;
    
    [ObservableProperty] 
    [property: EditableEnum("Use Switch", "", 0)]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;

    [ObservableProperty] [property: EditableColor("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: EditableColor("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: EditableColor("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: EditableColor("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;
    
    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Off;

    [JsonIgnore]
    public Light? Light => Parent?.Light(SwitchID);

    [JsonConstructor]
    public SwitchEntity() { }

    public SwitchEntity(Panel panel) : base(panel) { }
    public SwitchEntity(SwitchEntity entity) : base(entity) { }
    public override string EntityName => "Switch";

    public override Entity Clone() {
        return new SwitchEntity(this);
    }

}