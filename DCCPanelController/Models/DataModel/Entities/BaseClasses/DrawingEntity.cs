using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities.BaseClasses;

public abstract partial class DrawingEntity : Entity {
    protected DrawingEntity(Panel panel) : base(panel) { }
    protected DrawingEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
    protected DrawingEntity() { }

    [ObservableProperty] [property: Editable("Scale", "Size the object < 1 smaller > 1 bigger", 0, "Visibility")]
    private double _scale = 1.0;
    
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