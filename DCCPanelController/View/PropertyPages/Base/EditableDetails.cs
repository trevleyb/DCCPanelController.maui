using System.Reflection;

namespace DCCPanelController.View.PropertyPages.Base;

/// <summary>
///     Holds information about the editable properties in a provided object
/// </summary>
public record EditableDetails {
    public required EditableAttribute EditableAttribute { get; init; }
    public required PropertyInfo Info { get; init; }
    public required Type Type { get; init; }
    public required object Owner { get; init; }
    public required int Order { get; init; }
}