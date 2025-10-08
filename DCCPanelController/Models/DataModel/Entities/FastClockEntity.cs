using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class FastClockEntity : Entity, IDrawingEntity, IInteractiveEntity {
    [JsonConstructor]
    public FastClockEntity() { }
    public FastClockEntity(Panel panel) : this() => Parent = panel;
    public FastClockEntity(FastClockEntity entity) : base(entity) { }

    [ObservableProperty] [property: Editable("Type", "", 0, "Fastclock")]
    private FastClockTypeEnum _fastclockType = FastClockTypeEnum.Digital;
    
    [ObservableProperty] [property: Editable("Background", "", 1, "Fastclock")]
    private Color _backgroundColor = Colors.White;

    [ObservableProperty] [property: Editable("Border", "", 2, "Fastclock")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: Editable("Border Width", "", 3, Group = "Fastclock")]
    private int _borderWidth = 1;

    [ObservableProperty] [property: Editable("Hours:Min", "", 10, "Fastclock")]
    private Color _timeColor = Colors.Black;

    [ObservableProperty] [property: Editable("Seconds", "", 11, "Fastclock")]
    private Color _secondHandColor = Colors.Black;

    [ObservableProperty] [property: Editable("Ticks", "", 12, "Fastclock")]
    private Color _ticksColor = Colors.Black;

    [JsonIgnore] protected override int RotationFactor => 0;
    
    [JsonIgnore] public override string EntityName => "Fastclock";
    [JsonIgnore] public override string EntityDescription => "A representation of a Fast Clock";
    [JsonIgnore] public override string EntityInformation =>
        "The **Fastclock** allows you to show a clock that is syncronised with the fast clock from the DCC System.";

    public override Entity Clone() => new FastClockEntity(this);
}