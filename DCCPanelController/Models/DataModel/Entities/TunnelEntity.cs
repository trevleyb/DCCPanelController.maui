using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TunnelEntity : StraightEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("Tunnel Color", "Color of the Tunnel Entrance", 6, "Color")]
    private Color? _tunnelColor;

    [JsonConstructor]
    public TunnelEntity() { }

    public TunnelEntity(Panel panel) : this() => Parent = panel;

    public TunnelEntity(TunnelEntity entity) : base(entity) { }
    public override string EntityName => "Tunnel";
    public override string EntityDescription => "Tunnel Entrance/Exit Track";
    public override string EntityInformation =>
        "This is a normal **track** that shows an indicator to indicate that this is the entrance or exit of a tunnel. Normally track in a tunnel is marked as *dashed* to indicate it is hidden track.";

    public override Entity Clone() => new TunnelEntity(this);
}