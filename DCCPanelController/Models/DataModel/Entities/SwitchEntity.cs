using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : Entity,  IInteractiveEntity {

    [ObservableProperty] 
    [property: EditableLightSwitch("Light", "", 0)]
    private string _switchID = string.Empty;
    
    [ObservableProperty] 
    [property: EditableEnum("Use Switch", "", 0)]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;

    [ObservableProperty] private ButtonStateEnum _state = ButtonStateEnum.Off;

    [JsonIgnore]
    public Light? Light => Parent?.Light(SwitchID);

    [JsonConstructor]
    public SwitchEntity() { }

    public SwitchEntity(Panel panel) : base(panel) { }

    public SwitchEntity(SwitchEntity entity) : base(entity) {
        State = entity.State;
        SwitchStyle = entity.SwitchStyle;

    }
    public override string EntityName => "Switch";

    public override Entity Clone() {
        return new SwitchEntity(this);
    }

}