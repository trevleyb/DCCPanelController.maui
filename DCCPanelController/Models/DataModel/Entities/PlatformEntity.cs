using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PlatformEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableColor("Platform Color", "Color of the Platform", 6, "Track")]
    private Color? _platformColor;
    
    [JsonConstructor]
    public PlatformEntity() { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.PlatformTrack;
    public PlatformEntity(Panel panel) : this() {
        Parent = panel;
    }
    [JsonIgnore] protected override int RotationFactor => 90;

    public PlatformEntity(PlatformEntity entity) : base(entity) { }
    public override string EntityName => "Platform Track";

    public override Entity Clone() {
        return new PlatformEntity(this);
    }
}