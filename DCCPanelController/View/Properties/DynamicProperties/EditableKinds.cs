using CommunityToolkit.Maui.Converters;

namespace DCCPanelController.View.Properties.DynamicProperties;

public static class EditorKinds {
    public const string UniqueID = "uniqueID";
    public const string Text = "text";
    public const string Multiline = "multiline";
    public const string Password = "password";
    
    public const string Int = "int";
    public const string Number = "number";
    public const string Opacity = "opacity";
    
    public const string Toggle = "toggle";
    public const string Choice = "choice";
    public const string MultiSelect = "multiselect";
    
    public const string Enum = "enum";
    public const string EnumChoice = "enum.choice";
    public const string EnumRadio = "enum.radio";

    public const string Color = "color";
    public const string Date = "date";
    public const string TimeSpan = "timespan";
    public const string Url = "url";

    public const string Block = "block";
    public const string Route = "route";
    public const string Turnout = "turnout";
    public const string TurnoutState = "turnoutstate";
    public const string ButtonState = "buttonstate";
    public const string Image = "image";
    public const string Light = "light";

    public const string ButtonActions = "buttonactions";
    public const string TurnoutActions = "turnoutactions";

    public static void RegisterDefaults(IPropertyRendererRegistry registry) {
        registry.Register(EditorKinds.UniqueID, new UniqueIDRenderer());
        registry.Register(EditorKinds.Text, new TextRenderer());
        registry.Register(EditorKinds.Multiline, new MultilineTextRenderer());
        registry.Register(EditorKinds.Password, new PasswordRenderer());
        
        registry.Register(EditorKinds.Int, new IntRenderer());
        registry.Register(EditorKinds.Number, new NumberRenderer());
        registry.Register(EditorKinds.Opacity, new OpacityRenderer());
        
        registry.Register(EditorKinds.Toggle, new ToggleRenderer());
        registry.Register(EditorKinds.Choice, new ChoiceRenderer());
        registry.Register(EditorKinds.MultiSelect, new ChoiceRenderer());

        registry.Register(EditorKinds.Enum, new EnumRadioRenderer());
        registry.Register(EditorKinds.EnumChoice, new EnumChoiceRenderer());
        registry.Register(EditorKinds.EnumRadio,  new EnumRadioRenderer());

        registry.Register(EditorKinds.Color, new ColorPickerRenderer());
        registry.Register(EditorKinds.Date, new DateRenderer());
        registry.Register(EditorKinds.TimeSpan, new TimeSpanRenderer());
        registry.Register(EditorKinds.Url, new UrlRenderer());

        registry.Register(EditorKinds.Block, new BlockRenderer());
        registry.Register(EditorKinds.Route, new RouteRenderer());
        registry.Register(EditorKinds.Turnout, new TurnoutRenderer());
        registry.Register(EditorKinds.Light, new LightRenderer());

        registry.Register(EditorKinds.TurnoutState, new TurnoutStateRenderer());
        registry.Register(EditorKinds.ButtonState, new ButtonStateRenderer());

        registry.Register(EditorKinds.Image, new ImageRenderer());

        registry.Register(EditorKinds.ButtonActions, new ActionsButtonRenderer());
        registry.Register(EditorKinds.TurnoutActions, new ActionsTurnoutRenderer());

    }

    public static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;
    public static bool IsColorType(Type t) => t.FullName == "Microsoft.Maui.Graphics.Color" || t.Name.Equals("Color", StringComparison.Ordinal);

}

