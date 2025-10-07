using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class FastClockEntity : Entity, IDrawingEntity {
    [JsonConstructor]
    public FastClockEntity() { }
    public FastClockEntity(Panel panel) : this() => Parent = panel;
    public FastClockEntity(FastClockEntity entity) : base(entity) { }

    [ObservableProperty] [property: Editable("Type", "", 0, "Fastclock")]
    private FastClockTypeEnum _fastclockType = FastClockTypeEnum.Digital;
    
    [ObservableProperty] [property: Editable("Background", "", 0, "Fastclock")]
    private Color _backgroundColor = Colors.White;

    [ObservableProperty] [property: Editable("Border", "", 0, "Fastclock")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: Editable("Hours", "", 0, "Fastclock")]
    private Color _hoursColor = Colors.Black;

    [ObservableProperty] [property: Editable("Time", "", 0, "Fastclock")]
    private Color _timeColor = Colors.Black;

    [ObservableProperty] [property: Editable("SecondHand", "", 0, "Fastclock")]
    private Color _secondHandColor = Colors.Black;
    
    [JsonIgnore] protected override int RotationFactor => 0;
    
    [JsonIgnore] public override string EntityName => "Fastclock";
    [JsonIgnore] public override string EntityDescription => "A representation of a Fast Clock";
    [JsonIgnore] public override string EntityInformation =>
        "The **Fastclock** allows you to show a clock that is syncronised with the fast clock from the DCC System.";

    public override Entity Clone() => new FastClockEntity(this);
}