using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class BoolToVisibilityConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool booleanValue) {
            return booleanValue;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}