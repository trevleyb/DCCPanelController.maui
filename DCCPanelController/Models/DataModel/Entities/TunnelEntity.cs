using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class TunnelEntity : StraightEntity, ITrackEntity {
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