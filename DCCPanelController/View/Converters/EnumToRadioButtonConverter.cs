using System.Globalization;

namespace DCCPanelController.View.Converters;

public class EnumToRadioButtonConverter<T>(T enumValue) : IValueConverter
    where T : struct, Enum {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is T enumValue1 && enumValue1.Equals(enumValue);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool boolValue && boolValue) return enumValue;
        return Binding.DoNothing;
    }
}