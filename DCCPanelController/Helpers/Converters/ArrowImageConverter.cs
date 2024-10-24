using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class BooleanToArrowConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded) {
            return isExpanded ? "chevron_down.png" : "chevron_right.png";
        }
        return "arrow_right.png";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
