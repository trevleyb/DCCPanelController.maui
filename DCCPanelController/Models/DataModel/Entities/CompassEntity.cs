using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;
using ExCSS;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Entities;
  
public partial class CompassEntity : Entity {
    
    public CompassEntity() { }

    
    [ObservableProperty] [property: Editable("Block")] private string? _block;
    [ObservableProperty] [property: Editable("Button")] private ButtonStateEnum? _buttonState;
    [ObservableProperty] 
    [property: Editable("Choices")]
    [property: EditableParam("choices", new[] { "Auto", "Manual", "Off" })]
    private string? _choices;
    
    [ObservableProperty] 
    [property: Editable("MoreChoices", Choices = new[] {"One","Two","Three"})]
    private string? _moreChoices;

    [ObservableProperty] [property: Editable("Name", Group = "General")] private string? _name;
    [ObservableProperty] [property: Editable("Text", Group = "General", Description = "Enter some text dufus" ,EditorKind = "text")] private string? text;

    [ObservableProperty] [property: Editable("Color by Name One", Group = "Colors", Order = 0)] private Color _colorOne;
    [ObservableProperty] [property: Editable("Color by Name Two", Group = "Colors", Order = 0)] private Color _secondColor;
    [ObservableProperty] [property: Editable("Color by Another ", Group = "Colors", EditorKind  = EditorKinds.Color)] private Color _another;
    
    [ObservableProperty] [property: Editable("Date")] private DateOnly _date;
    
    [ObservableProperty] [property: Editable("Enum Default")] private TextAttributeEnum? _textAttribute1;
    [ObservableProperty] [property: Editable("Enum Radio", EditorKind = EditorKinds.EnumRadio)] private TextAttributeEnum? _textAttribute2;
    [ObservableProperty] [property: Editable("Enum Choice", EditorKind = EditorKinds.EnumChoice)] private TextAttributeEnum? _textAttribute3;
    
    [ObservableProperty] [property: Editable("Image")] private string? _image;

    [ObservableProperty] [property: Editable("Integer", Group="Numbers")] private int _integer;
    [ObservableProperty] [property: Editable("Opacity", Group="Numbers")] private double _opacity;
    
    [ObservableProperty] [property: Editable("Double Number", Group="Numbers")] private double _dNumber;
    [ObservableProperty] [property: Editable("Float Number", Group="Numbers")] private float _fNumber;
    [ObservableProperty] [property: Editable("Decimal Number", Group="Numbers")] private decimal _decNumber;

    [ObservableProperty] [property: Editable("Multiline", Group="Others", EditorKind = "multiline")] private string? multiline;
    [ObservableProperty] [property: Editable("Password", Group="Others", EditorKind = "password")] private string? password;
    [ObservableProperty] [property: Editable("Number", Group="Others", EditorKind = "number")] private int _number;
    [ObservableProperty] [property: Editable("Bool", Group="Others" )] private bool _bool;
    [ObservableProperty] [property: Editable("Toggle", Group="Others", EditorKind = "toggle")] private bool toggle;
    [ObservableProperty] [property: Editable("Choice", Group="Others" )] private TextAttributeEnum _textStyle;
    [ObservableProperty] [property: Editable("Color", Group="Others", Order = 0)] private Color _color;
    [ObservableProperty] [property: Editable("Time", Group="Others")] private TimeOnly _time;
    [ObservableProperty] [property: Editable("URL", Group="Others")] private Url _url;
    
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