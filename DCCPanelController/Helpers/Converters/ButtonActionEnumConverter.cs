using System.Globalization;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Helpers.Converters;

public class ButtonActionEnumConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value?.ToString() ?? "Unknown Value";
    }

    // Maps the string back to the corresponding enum
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string strValue) return Enum.TryParse(strValue, out ButtonStateEnum result) ? result : null;
        return ButtonStateEnum.Unknown;
    }
}