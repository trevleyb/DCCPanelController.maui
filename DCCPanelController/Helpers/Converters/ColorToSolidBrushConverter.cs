using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class ColorToSolidColorConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is Color color) return new SolidColorBrush(color);
        return value ;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is SolidColorBrush brush) return brush.Color;
        return value;
    }
}