using DCCPanelController.View.Properties.DynamicProperties.Renderers;

namespace DCCPanelController.View.Properties.DynamicProperties;

public static class EditorKinds {
    public const string UniqueID  = "uniqueID";
    public const string Text      = "text";
    public const string Multiline = "multiline";
    public const string Password  = "password";

    public const string Int     = "int";
    public const string Number  = "number";
    public const string Opacity = "opacity";

    public const string Toggle      = "toggle";
    public const string Choice      = "choice";
    public const string MultiSelect = "multiselect";

    public const string Enum       = "enum";
    public const string EnumChoice = "enum.choice";
    public const string EnumRadio  = "enum.radio";

    public const string Color    = "color";
    public const string Date     = "date";
    public const string TimeSpan = "timespan";
    public const string Url      = "url";

    public const string Block        = "block";
    public const string Route        = "route";
    public const string Turnout      = "turnout";
    public const string TurnoutState = "turnoutstate";
    public const string ButtonState  = "buttonstate";
    public const string Image        = "image";
    public const string Light        = "light";

    public const string ButtonActions  = "buttonactions";
    public const string TurnoutActions = "turnoutactions";

    public static void RegisterDefaults(IPropertyRendererRegistry registry) {
        registry.Register(UniqueID, new UniqueIDRenderer());
        registry.Register(Text, new TextRenderer());
        registry.Register(Multiline, new MultilineTextRenderer());
        registry.Register(Password, new PasswordRenderer());

        registry.Register(Int, new IntRenderer());
        registry.Register(Number, new NumberRenderer());
        registry.Register(Opacity, new OpacityRenderer());

        registry.Register(Toggle, new ToggleRenderer());
        registry.Register(Choice, new ChoiceRenderer());
        registry.Register(MultiSelect, new ChoiceRenderer());

        registry.Register(Enum, new EnumRadioRenderer());
        registry.Register(EnumChoice, new EnumChoiceRenderer());
        registry.Register(EnumRadio, new EnumRadioRenderer());

        registry.Register(Color, new ColorPickerRenderer());
        registry.Register(Date, new DateRenderer());
        registry.Register(TimeSpan, new TimeSpanRenderer());
        registry.Register(Url, new UrlRenderer());

        registry.Register(Block, new BlockRenderer());
        registry.Register(Route, new RouteRenderer());
        registry.Register(Turnout, new TurnoutRenderer());
        registry.Register(Light, new LightRenderer());

        registry.Register(TurnoutState, new TurnoutStateRenderer());
        registry.Register(ButtonState, new ButtonStateRenderer());

        registry.Register(Image, new ImageRenderer());

        registry.Register(ButtonActions, new ActionsButtonRenderer());
        registry.Register(TurnoutActions, new ActionsTurnoutRenderer());
    }

    public static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;
    public static bool IsColorType(Type t) => t.FullName == "Microsoft.Maui.Graphics.Color" || t.Name.Equals("Color", StringComparison.Ordinal);
}