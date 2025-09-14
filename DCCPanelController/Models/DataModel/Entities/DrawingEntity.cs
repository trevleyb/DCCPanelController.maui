using System.Text.Json.Serialization;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract class DrawingEntity : Entity {
    protected DrawingEntity(Panel panel) : base(panel) { }
    protected DrawingEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
    protected DrawingEntity() { }

    [Editable("Width in Cells", "", 1, "Dimensions")]
    [JsonIgnore] public int DrawingWidth {
        get => Width;
        set => Width = value;
    }

    [Editable("Height in Cells", "", 2, "Dimensions")]
    [JsonIgnore] public int DrawingHeight {
        get => Height;
        set => Height = value;
    }
}