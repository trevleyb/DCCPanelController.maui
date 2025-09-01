using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Views.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TunnelEntity : StraightEntity, ITrackEntity {
    [ObservableProperty] [property: EditableColor("Tunnel Color", "Color of the Tunnel Entrance", 6, "Track")]
    private Color? _tunnelColor;

    [JsonConstructor]
    public TunnelEntity() { }

    public TunnelEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TunnelEntity(TunnelEntity entity) : base(entity) { }
    public override string EntityName => "Tunnel Entrance Track";

    public override Entity Clone() {
        return new TunnelEntity(this);
    }
}