using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CornerContinuationEntity : Entity {
    public override string Name => "Corner Track";
    [ObservableProperty] private TerminatorStyleEnum _continuationStyle = TerminatorStyleEnum.Arrow;
    
    [JsonConstructor]
    private CornerContinuationEntity() {}
    public CornerContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CornerContinuationEntity(CornerContinuationEntity entity) : base(entity) {
        ContinuationStyle = entity.ContinuationStyle;
    }
    public override Entity Clone() => new CornerContinuationEntity(this);
}