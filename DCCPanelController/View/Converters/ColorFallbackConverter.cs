using System.Globalization;

namespace DCCPanelController.View.Converters;

public class ColorFallbackConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var fallbackValue = parameter as Color ?? Colors.White;
        var color = value as Color;
        return color ?? fallbackValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var fallbackValue = parameter as Color ?? Colors.White;
        var color = value as Color;
        return color ?? fallbackValue;
    }
}