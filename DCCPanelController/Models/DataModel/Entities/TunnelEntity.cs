
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TunnelEntity : StraightEntity, ITrackEntity {
    public override string EntityName => "Tunnel Track";
    
    [JsonConstructor]
    public TunnelEntity() {}
    public TunnelEntity(Panel panel) : this() {
        Parent = panel;
    }
    public TunnelEntity(TunnelEntity entity) : base(entity) {}
    public override Entity Clone() => new TunnelEntity(this);
}