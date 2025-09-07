namespace DCCPanelController.View.Properties.DynamicProperties;

public static class EditorKinds {
    public const string Text = "text";
    public const string Multiline = "multiline";
    public const string Password = "password";
    public const string Int = "int";
    public const string Number = "number";
    public const string Toggle = "toggle";
    public const string Choice = "choice";
    public const string MultiSelect = "multiselect";
    public const string Color = "color";
    public const string Date = "date";
    public const string TimeSpan = "timespan";
    public const string Url = "url";
    public const string EnumChoice = "enum.choice";
    public const string EnumButtons = "enum.buttons";
    public const string EnumRadio = "enum.radio";
    public const string Block = "block";
    public const string Occupancy = "block";
    public const string Actions = "actions";
    public const string ButtonActions = "buttonactions";
    public const string TurnoutActions = "turnoutactions";
    public const string Button = "button";
    public const string Image = "image";
    public const string Opacity = "opacity";
    public const string Route = "route";
    public const string Switch = "switch";
    public const string Turnout = "turnout";

    public static int LabelWidth(string? kind = "default") =>  kind is not null && EditorWidths.ContainsKey(kind) ? EditorWidths[kind].LabelWidth : EditorWidths["default"].LabelWidth;
    public static int FieldWidth(string? kind = "default") =>  kind is not null && EditorWidths.ContainsKey(kind) ? EditorWidths[kind].FieldWidth : EditorWidths["default"].LabelWidth;
    public static int FieldHeight(string? kind = "default") =>  kind is not null && EditorWidths.ContainsKey(kind) ? EditorWidths[kind].FieldHeight : EditorWidths["default"].FieldHeight;

    public static Dictionary<string, (int LabelWidth, int FieldWidth, int FieldHeight)> EditorWidths = new() {
        ["default"] = (150, 250, 35),
        [EditorKinds.Text] = (150, 250, 35),
        [EditorKinds.Multiline] = (150, 250, 35),
        [EditorKinds.Password] = (150, 250, 35), 
        [EditorKinds.Int] = (150, 100, 35),
        [EditorKinds.Number] = (150, 100, 35), 
        [EditorKinds.Toggle] = (150, 100, 35), 
        [EditorKinds.Choice] = (150, 250, 35),
        [EditorKinds.MultiSelect] = (150, 250, 35),
        [EditorKinds.Color] = (150, 250, 35),
        [EditorKinds.Date] = (150, 250, 35),
        [EditorKinds.TimeSpan] = (150, 250, 35), 
        [EditorKinds.Url] = (150, 250, 35),
        [EditorKinds.EnumChoice] = (150, 250, 35),
        [EditorKinds.EnumButtons] = (150, 250, 35),
        [EditorKinds.EnumRadio] = (150, 250, 35),
        [EditorKinds.Block] = (150, 250, 35),
        [EditorKinds.Occupancy] = (150, 250, 35),
        [EditorKinds.Actions] = (150, 250, 35),
        [EditorKinds.ButtonActions] = (150, 250, 35),
        [EditorKinds.TurnoutActions] = (150, 250, 35),
        [EditorKinds.Button] = (150, 250, 35),
        [EditorKinds.Image] = (150, 250, 35),
        [EditorKinds.Opacity] = (150, 250, 35),
        [EditorKinds.Route] = (150, 250, 35),
        [EditorKinds.Switch] = (150, 250, 35), 
        [EditorKinds.Turnout] = (150, 250, 35)
    };
}
