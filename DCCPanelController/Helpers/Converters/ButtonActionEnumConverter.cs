using System.Globalization;
using DCCPanelController.Model;

namespace DCCPanelController.Helpers.Converters;

public class ButtonActionEnumConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is ButtonStateEnum stateEnum) {
            // Convert enum to the matching string defined in Picker.Items
            return stateEnum switch {
                ButtonStateEnum.Active   => "Active (on)",
                ButtonStateEnum.Inactive => "Inactive (off)",
                ButtonStateEnum.Toggle   => "Toggle",
                ButtonStateEnum.Leave    => "Leave As-Is",
                _                        => "Unknown"
            };
        }

        return value?.ToString() ?? string.Empty;
    }

    // Maps the string back to the corresponding enum
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string strValue) {
            // Convert the string from the Picker back to the correct enum value
            return strValue switch {
                "Active (on)"    => ButtonStateEnum.Active,
                "Inactive (off)" => ButtonStateEnum.Inactive,
                "Toggle"         => ButtonStateEnum.Toggle,
                "Leave As-Is"    => ButtonStateEnum.Leave,
                _                => throw new InvalidOperationException($"Unknown option: {strValue}")
            };
        }

        throw new InvalidOperationException("Invalid value for conversion");
    }
}