using System.Globalization;
using DCCPanelController.Model;

namespace DCCPanelController.Helpers.Converters;

public class TurnoutActionEnumConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value?.ToString() ?? "Unknown Value";
    }

    // Maps the string back to the corresponding enum
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string strValue) return Enum.TryParse(strValue, out TurnoutStateEnum result) ? result : null;
        return TurnoutStateEnum.Unknown;
    }
}