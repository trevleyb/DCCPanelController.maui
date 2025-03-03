using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class ExpandRotationConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool isExpanded) {
            return isExpanded ? 0 : -90; // 0 degrees for expanded, -90 for collapsed
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int isExpanded) {
            return isExpanded >=0; // 0 degrees for expanded, -90 for collapsed
        }
        return false;
    }
}