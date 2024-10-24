using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class FallbackValueConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var fallbackValue = parameter as string;
        var text = value as string;

        return string.IsNullOrEmpty(text) ? fallbackValue : text;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}