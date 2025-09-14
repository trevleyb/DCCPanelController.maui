using System.Globalization;

namespace DCCPanelController.View.Converters;

public class BoolToChevronConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool isExpanded) {
            return isExpanded ? "chevron_circle_down.png" : "chevron_circle_right.png"; // Material Icons: expand_more : chevron_right
        }
        return"chevron_circle_left.png"; // Default to right chevron
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ((IValueConverter)this).ConvertBack(value, targetType, parameter, culture);
}