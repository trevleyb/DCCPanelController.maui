using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class BridgeEntity : StraightEntity, ITrackEntity {
    [ObservableProperty]
    [property: Editable("Bridge Color", "Color of the Bridge rails", 6, "Color")]
    private Color? _bridgeColor;

    [JsonConstructor]
    public BridgeEntity() { }

    public BridgeEntity(Panel panel) : this() {
        Parent = panel;
    }

    public BridgeEntity(BridgeEntity entity) : base(entity) { }
    public override string EntityName => "Bridge";
    public override string EntityDescription => "Bridge Rails Track";
    public override string EntityInformation => "";

    public override Entity Clone() {
        return new BridgeEntity(this);
    }
}