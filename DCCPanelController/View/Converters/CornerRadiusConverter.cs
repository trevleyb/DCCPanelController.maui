using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class CornerRadiusConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int val) return new CornerRadius(val);
        return new CornerRadius(0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is CornerRadius radius) return radius.TopLeft;
        return value;
    }
}