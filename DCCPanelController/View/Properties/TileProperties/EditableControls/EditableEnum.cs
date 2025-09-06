using System.Reflection;
using DCCPanelController.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableEnum(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            // Get the enum type from the property
            var enumType = info.PropertyType;
            var enumValues = Enum.GetValues(enumType).Cast<object>().ToList();
            var enumNames = enumValues.Select(v => v.ToString()).ToList();

            // Add a "Mixed Values" option when dealing with mixed states
            if (HasMixedValues) { // You'd need to pass this information somehow{
                enumNames.Insert(0, "Mixed Values");
                enumValues.Insert(0, null!); // Special marker for mixed state
            }

            // Handle nullable enums
            if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            }

            if (!enumType.IsEnum) {
                throw new InvalidOperationException($"Property '{info.Name}' is not an enum type.");
            }

            return CreateRadioGroupForEnum(Label, enumNames, enumValues, enumType, owner, info, HasMixedValues);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create enum control for {infoName}: {Message}", info.Name, e.Message);
            return null;
        }
    }

    private IView? CreateRadioGroupForEnum(string name, List<string?> enumNames, List<object> enumValues, Type enumType, object owner, PropertyInfo info, bool hasMixedvalues) {
        if (owner == null) throw new ArgumentNullException(nameof(owner), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(info.Name)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(info.Name));

        var initialValue = Value;
        var radioGroup = new FlexLayout() {
            Direction = FlexDirection.Column,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.Start,
            AlignItems = FlexAlignItems.Start,
            AlignContent = FlexAlignContent.Start,
            Margin = new Thickness(-5, 0, 0, 0)
        };
        
        for (var i = 0; i < enumNames.Count; i++) {
            var rb = new RadioButton {
                // Make the label NoWrap so it measures to its text width
                Content = new Label {
                    BackgroundColor = Colors.AliceBlue,
                    Text = enumNames[i] ?? "Unknown",
                    LineBreakMode = LineBreakMode.NoWrap,
                    VerticalTextAlignment = TextAlignment.Center
                },
                Value = enumValues[i] ?? 0,
                BorderWidth = 0,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(6, 4)
            };

            // Critical: prevent full-row expansion
            FlexLayout.SetBasis(rb, FlexBasis.Auto);
            FlexLayout.SetGrow(rb, 0);
            FlexLayout.SetShrink(rb, 1);
            FlexLayout.SetAlignSelf(rb, FlexAlignSelf.Start);

            if (HasMixedValues) {
                if (enumValues[i] == null!) rb.IsChecked = true;
            } else {
                var currentValue = info.GetValue(owner);
                rb.IsChecked = enumValues[i]?.Equals(currentValue) ?? false;
            }

            rb.CheckedChanged += (sender, args) => {
                if (sender is RadioButton button && args.Value) {
                    var buttonVal = button.Value;
                    PropertyHelper.SetEnumPropertyValue(owner, info.Name, buttonVal);
                    SetModified(buttonVal?.ToString() != initialValue?.ToString());
                }
            };

            radioGroup.Children.Add(rb);
        }

        SetModified(false);
        return CreateGroupCell(radioGroup);
    }
}