using System.Reflection;

namespace DCCPanelController.Helpers.Attributes;

/// <summary>
/// Holds information about the editable properties in a provided object
/// </summary>
public record EditablePropertyDetails {
        public required EditablePropertyAttribute Attribute { get; init; }
        public required PropertyInfo PropertyInfo { get; init; }
        public required Type PropertyType { get; init; }
        public required object PropertyOwner { get; init; }
        public required int ReadOrder { get; init; }
}