using System;

namespace DCCPanelController.View.Properties.DynamicProperties
{
    /// <summary>
    /// Attach to the TARGET property. The rule string refers to other properties
    /// on the same owner object(s).
    /// Examples:
    ///   [EditableRules(IsEnabledWhen = nameof(IsAvailable))]
    ///   [EditableRules(IsEnabledWhen = nameof(IsAvailable) + " == true")]
    ///   [EditableRules(IsEnabledWhen = nameof(Mode) + " == Pro && " + nameof(UseAdvanced) + " == true")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EditableRulesAttribute : Attribute
    {
        /// <summary>
        /// Simple boolean/compare expression over sibling properties.
        /// Supported: identifiers (property names), literals (true/false/numbers/enum names),
        /// operators: ==, !=, &&, ||, unary !. No method calls. Case-insensitive for booleans/enums.
        /// </summary>
        public string? IsEnabledWhen { get; init; }

        /// <summary>
        /// If true (default), the rule ANDs with the renderer's baseline IsEnabled.
        /// </summary>
        public bool RespectBaseline { get; init; } = true;
    }
}
