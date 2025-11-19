using System.Globalization;
using System.ComponentModel;

namespace DCCPanelController.View.Converters;

public class EnumToLabelConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is not Enum enumValue ? value?.ToString() : GetEnumDescription(enumValue);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        // Generic string-to-enum conversion based on display name is ambiguous and usually not needed for labels.
        throw new NotImplementedException();
    }

    private static string GetEnumDescription(Enum value) {
        var field = value.GetType().GetField(value.ToString());
        if (field is null) return value.ToString();
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
            return attribute.Description;
        }
        return value.ToString();
    }
}