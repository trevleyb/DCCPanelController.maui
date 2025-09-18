using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class SwitchEntity : ButtonEntity, IInteractiveEntity {
    [ObservableProperty] [property: Editable("Button Size", Order = 2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty]
    [property: Editable("Light", "Select the ID of the light controlled by this button", 1, "General", EditorKind = EditorKinds.Light)]
    private string _id = string.Empty;

    [ObservableProperty] 
    private ButtonStateEnum _state = ButtonStateEnum.Off;

    [ObservableProperty]
    [property: Editable("Switch Style", Order = 3)]
    private SwitchStyleEnum _switchStyle = SwitchStyleEnum.Light;

    [JsonConstructor]
    public SwitchEntity() { }
    public SwitchEntity(Panel panel) : base(panel) { }
    public SwitchEntity(SwitchEntity entity) : base(entity) { }

    [JsonIgnore]
    public Light? Light => Parent?.Light(Id);

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Switch";
    public override string EntityDescription => "On/Off Switch";
    public override string EntityInformation => "This button turns on or off a __switch__ normally controlling a light on the layout. ";

    public override Entity Clone() => new SwitchEntity(this);

    public override string ToString() => Id;
}