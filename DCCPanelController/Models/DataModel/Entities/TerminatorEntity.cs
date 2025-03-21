using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TerminatorEntity : TrackEntity, ITrackEntity {
    public override string Name => "Terminator Track";

    [ObservableProperty] [property: EditableTrackTerminator("Terminator",  group: "Track")] 
    private TrackTerminatorEnum _terminatorStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public TerminatorEntity() { }
    public TerminatorEntity(Panel panel) : this() {
        Parent = panel;
    }
    public TerminatorEntity(TerminatorEntity entity) : base(entity) {
        TerminatorStyle = entity.TerminatorStyle;
    }
    public override Entity Clone() => new TerminatorEntity(this);
}