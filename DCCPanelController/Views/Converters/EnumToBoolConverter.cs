using System.Globalization;

namespace DCCPanelController.Views.Converters;

public class EnumToBoolConverter<T>(T enumValue) : IValueConverter where T : struct {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is T enumValue1 && enumValue1.Equals(enumValue);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool and true) {
            return enumValue;
        }

        return Binding.DoNothing;
    }
}