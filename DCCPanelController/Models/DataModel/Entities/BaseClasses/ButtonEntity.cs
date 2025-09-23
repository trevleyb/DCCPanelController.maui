using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class ButtonEntity : Entity {
    [ObservableProperty] [property: Editable("Button Off", "Color of the Button when the button is **OFF**", 5, Group = "Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: Editable("Border Off", "Color of the Border when the button is **OFF**", 6, Group = "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("Button On", "Color of the Button when the button is **ON**", 3, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("Border On", "Color of the Border when the button is **ON**", 4, Group = "Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty] [property: Editable("Button UnSet", "Color used if the state of the button is **Unknown**", 1, Group = "Colors")]
    private Color? _colorUnknown;

    [ObservableProperty] [property: Editable("Border UnSet", "Color used for the border if the state of the button is **Unknown**", 2, Group = "Colors")]
    private Color? _colorUnknownBorder;

    protected ButtonEntity(Panel panel) : base(panel) { }
    protected ButtonEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
    protected ButtonEntity() { }
}