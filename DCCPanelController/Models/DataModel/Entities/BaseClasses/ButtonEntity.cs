using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class ButtonEntity : Entity {
    [ObservableProperty] [property: Editable("Button (OFF)", "Color of the Button when the button is **OFF**", 5, Group = "Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: Editable("Border (OFF)", "Color of the Border when the button is **OFF**", 6, Group = "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("Button (ON)", "Color of the Button when the button is **ON**", 3, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("Border (ON)", "Color of the Border when the button is **ON**", 4, Group = "Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty] [property: Editable("Button (Unknown)", "Color used if the state of the button is **Unknown**", 1, Group = "Colors")]
    private Color? _colorUnknown;

    [ObservableProperty] [property: Editable("Border (Unknown)", "Color used for the border if the state of the button is **Unknown**", 2, Group = "Colors")]
    private Color? _colorUnknownBorder;

    protected ButtonEntity(Panel panel) : base(panel) { }
    protected ButtonEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
    protected ButtonEntity() { }
}