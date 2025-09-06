using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyContext {
    public PropertyRow Row { get; }
    public AppMode Mode { get; }
    public CultureInfo Culture { get; }
    public string EditorKind { get; internal set; } = "text"; // resolved kind, set by FormContext
    public System.Collections.Generic.IReadOnlyDictionary<string, object> Params => Row.Field.Meta.Parameters;

    public PropertyContext(PropertyRow row, AppMode mode, CultureInfo? culture = null) {
        Row = row;
        Mode = mode;
        Culture = culture ?? CultureInfo.InvariantCulture;
    }
}