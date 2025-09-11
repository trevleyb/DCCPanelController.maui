using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class ButtonEntity : Entity {
     protected ButtonEntity(Panel panel) : base(panel) { }
     protected ButtonEntity(Entity entity, params string[] excludeProperties) : base(entity, excludeProperties) { }
     protected ButtonEntity() { }

     [ObservableProperty] [property: Editable("Button Color", "Color used if the state of the button is **Unknown**", 1, Group="Colors")]
     private Color? _colorUnknown;

     [ObservableProperty] [property: Editable("Border Color", "Color used for the border if the state of the button is **Unknown**", 2, Group="Colors")]
     private Color? _colorUnknownBorder;

     [ObservableProperty] [property: Editable("Button Color (ON)", "Color of the Button when the button is **ON**", 3, "Colors")]
     private Color? _colorOn;

     [ObservableProperty] [property: Editable("Border Color (ON)", "Color of the Border when the button is **ON**", 4, Group="Colors")]
     private Color? _colorOnBorder;

     [ObservableProperty] [property: Editable("Button Color (OFF)", "Color of the Button when the button is **OFF**", 5, Group="Colors")]
     private Color? _colorOff;

     [ObservableProperty] [property: Editable("Border Color (OFF)", "Color of the Border when the button is **OFF**", 6, Group="Colors")]
     private Color? _colorOffBorder;
    

}