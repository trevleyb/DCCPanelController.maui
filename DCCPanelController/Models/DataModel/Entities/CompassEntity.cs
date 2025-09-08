using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.DynamicProperties;
using ExCSS;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Entities;
  
public partial class CompassEntity : Entity {
    
    public CompassEntity() { }

    [ObservableProperty] [property: Editable("Block", Group="Control")] private string? _block;
    [ObservableProperty] [property: Editable("Route", Group="Control")] private string? _route;
    [ObservableProperty] [property: Editable("Light", Group="Control")] private string? _light;
    [ObservableProperty] [property: Editable("Button", Group="Control")] private ButtonStateEnum? _buttonX;
    [ObservableProperty] [property: Editable("Turnout", Group="Control")] private TurnoutStateEnum? _turnoutX;

    [ObservableProperty] [property: Editable("Choices", Choices = new[] { "Auto", "Manual", "Off" }, Group="Choices")]
    private string? _choices;
    
    [ObservableProperty] [property: Editable("MoreChoices", EditorKind = EditorKinds.Choice,  Width = 100 ,Choices = new[] {"One","Two","Three"}, Group="Choices")]
    private string? _moreChoices;

    [ObservableProperty] [property: Editable("AutoChoice", Width=300, Choices = new[] {"One1","Two2","Three3"}, Group="Choices")]
    private string? _autoChoices;

    [ObservableProperty] [property: Editable("Style", Group="Choices" )] private TextAttributeEnum _textStyle;
    [ObservableProperty] [property: Editable("Style", Width=200, Group="Choices" )] private TextAttributeEnum _textStyle2;
    [ObservableProperty] [property: Editable("State", Group="Choices", EditorKind = EditorKinds.EnumRadio)] private ButtonStateEnum _state;

    [ObservableProperty] [property: Editable("Name1", Width=100,  Group = "General")] private string? _name1;
    [ObservableProperty] [property: Editable("Name2", Width=200,  Group = "General")] private string? _name2;
    [ObservableProperty] [property: Editable("Name3", Width=300,  Group = "General")] private string? _name3;
    [ObservableProperty] [property: Editable("Name4", Width=400,  Group = "General")] private string? _name4;
    [ObservableProperty] [property: Editable("Name5", Width=500,  Group = "General")] private string? _name5;
    [ObservableProperty] [property: Editable("Name0", Width=-1,  Group = "General")] private string? _name0;
    [ObservableProperty] [property: Editable("Text", Group = "General", Description = "Enter some text dufus" ,EditorKind = "text")] private string? text;

    [ObservableProperty] [property: Editable("Color by Name One", Group = "Colors", Order = 0)] private Color _colorOne;
    [ObservableProperty] [property: Editable("Color by Name Two", Group = "Colors", Order = 0)] private Color _secondColor;
    [ObservableProperty] [property: Editable("Color by Another ", Group = "Colors", EditorKind  = EditorKinds.Color)] private Color _another;
    [ObservableProperty] [property: Editable("Color", Group="Others", Order = 0)] private Color _color;

    [ObservableProperty] [property: Editable("Enum Default1", Group="ENUMS")] private TextAttributeEnum? _textAttribute1;
    [ObservableProperty] [property: Editable("Enum Default2", Group="ENUMS")] private TextAlignmentHorizontalEnum? _textHAlight;
    [ObservableProperty] [property: Editable("Enum Default3", Group="ENUMS")] private TextAlignmentVerticalEnum? _textVAlight;
    [ObservableProperty] [property: Editable("Enum Radio", Group="ENUMS", EditorKind = EditorKinds.EnumRadio)] private TextAttributeEnum? _textAttribute2;
    [ObservableProperty] [property: Editable("Enum Choice", Group="ENUMS", EditorKind = EditorKinds.EnumChoice)] private TextAttributeEnum? _textAttribute3;

    [ObservableProperty] [property: Editable("Integer Max", Max = 25, Group="Numbers")] private int _integer1;
    [ObservableProperty] [property: Editable("Integer Min", Min=10, Group="Numbers")] private int _integer2;
    [ObservableProperty] [property: Editable("Integer Max Min", Max=25, Min=10, Group="Numbers")] private int _integer3;
    
    [ObservableProperty] [property: Editable("Integer", Group="Numbers")] private int _integer21;
    [ObservableProperty] [property: Editable("Integer", Max=100, Min=20, Step=15, Group="Numbers")] private int _integer20;
    [ObservableProperty] [property: Editable("Opacity", Group="Numbers")] private double _opacity22;

    [ObservableProperty] [property: Editable("Opacity", Max=0.80, Min=0.40, Step=.05, Group="Numbers")] private double _opacity;
    
    [ObservableProperty] [property: Editable("Double Number", Group="Numbers")] private double _dNumber;
    [ObservableProperty] [property: Editable("Float Number", Group="Numbers")] private float _fNumber;
    [ObservableProperty] [property: Editable("Decimal Number", Group="Numbers")] private decimal _decNumber;

    [ObservableProperty] [property: Editable("ID", Group="Others")] private string _id;
    [ObservableProperty] [property: Editable("Date", Group="Others")] private DateOnly _date;
    [ObservableProperty] [property: Editable("Image", Group="Others")] private string? _image;
    [ObservableProperty] [property: Editable("Multiline", Group="Others", EditorKind = "multiline")] private string? multiline;
    [ObservableProperty] [property: Editable("Password", Group="Others", EditorKind = "password")] private string? password;
    [ObservableProperty] [property: Editable("Number", Group="Others", EditorKind = "number")] private int _number;
    [ObservableProperty] [property: Editable("Bool", Group="Others" )] private bool _bool;
    [ObservableProperty] [property: Editable("Toggle", Group="Others", EditorKind = "toggle")] private bool toggle;
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