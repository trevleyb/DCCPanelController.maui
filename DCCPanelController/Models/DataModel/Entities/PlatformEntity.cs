using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PlatformEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("Platform Color", "Color of the Platform", 6, "Color")]
    private Color? _platformColor;

    [JsonConstructor]
    public PlatformEntity() { }

    public PlatformEntity(Panel panel) : this() {
        Parent = panel;
    }

    public PlatformEntity(PlatformEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.PlatformTrack;
    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Platform";
    public override string EntityDescription => "Platform Track";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new PlatformEntity(this);
    }
}