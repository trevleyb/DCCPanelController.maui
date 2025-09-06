using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;
using ExCSS;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Entities;
  
public partial class CompassEntity : Entity {
    
    public CompassEntity() { }

    [ObservableProperty] [property: Editable("Name")] private string? _name;
    [ObservableProperty] [property: Editable("Text", Description = "Enter some text dufus" ,EditorKind = "text")] private string? text;
    [ObservableProperty] [property: Editable("Multiline", EditorKind = "multiline")] private string? multiline;
    [ObservableProperty] [property: Editable("Password", EditorKind = "password")] private string? password;
    [ObservableProperty] [property: Editable("Integer")] private int _integer;
    [ObservableProperty] [property: Editable("Number", EditorKind = "number")] private int _number;
    [ObservableProperty] [property: Editable("Bool", Group = "Special")] private bool _bool;
    [ObservableProperty] [property: Editable("Toggle", EditorKind = "toggle")] private bool toggle;
    [ObservableProperty] [property: Editable("Choice")] private TextAttributeEnum _textStyle;
    [ObservableProperty] [property: Editable("Color", Order = 0)] private Color _color;
    [ObservableProperty] [property: Editable("Date")] private DateOnly _date;
    [ObservableProperty] [property: Editable("Time")] private TimeOnly _time;
    [ObservableProperty] [property: Editable("URL")] private Url _url;
    
    public CompassEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;
    
    public CompassEntity(CompassEntity entity) : base(entity) { }
    public override string EntityName => "Compass";
    public override string EntityDescription => "Directional Compass";
    
    public override Entity Clone() {
        return new CompassEntity(this);
    }
}