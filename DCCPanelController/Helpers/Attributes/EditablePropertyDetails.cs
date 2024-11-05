using System.Reflection;

namespace DCCPanelController.Helpers.Attributes;

/// <summary>
/// Holds information about the editable properties in a provided object
/// </summary>
public record EditablePropertyDetails {
        public required EditablePropertyAttribute Attribute { get; init; }
        public required PropertyInfo Info { get; init; }
        public required Type Type { get; init; }
        public required object Owner { get; init; }
        public required int Order { get; init; }
}