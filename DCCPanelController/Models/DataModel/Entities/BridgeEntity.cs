using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class BridgeEntity : StraightEntity, ITrackEntity {
    [JsonConstructor]
    public BridgeEntity() { }

    [ObservableProperty] 
    [property: EditableColor("Bridge Color", "Color of the Bridge rails", 6, "Track")]
    private Color? _bridgeColor;
    
    public BridgeEntity(Panel panel) : this() {
        Parent = panel;
    }

    public BridgeEntity(BridgeEntity entity) : base(entity) { }
    public override string EntityName => "Bridge Track";

    public override Entity Clone() {
        return new BridgeEntity(this);
    }
}