namespace DCCPanelController.Models.DataModel.Entities.BaseClasses;

public abstract class DrawingEntity : Entity {
    protected DrawingEntity(Panel panel) : base(panel) { }
    protected DrawingEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
    protected DrawingEntity() { }

    // [Editable("Width in Cells", "Width of this component. Use the Size tool to resize.", 1, "Dimensions")]
    // [JsonIgnore] public int DrawingWidth {
    //     get => Width;
    //     set => Width = value;
    // }
    //
    // [Editable("Height in Cells", "Height of this component. Use the Size tool to resize.", 2, "Dimensions")]
    // [JsonIgnore] public int DrawingHeight {
    //     get => Height;
    //     set => Height = value;
    // }
}