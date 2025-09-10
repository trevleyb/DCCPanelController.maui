using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TunnelEntity : StraightEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("Tunnel Color", "Color of the Tunnel Entrance", 6, "Color")]
    private Color? _tunnelColor;

    [JsonConstructor]
    public TunnelEntity() { }

    public TunnelEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TunnelEntity(TunnelEntity entity) : base(entity) { }
    public override string EntityName => "Tunnel";
    public override string EntityDescription => "Tunnel Entrance/Exit Track";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new TunnelEntity(this);
    }
}