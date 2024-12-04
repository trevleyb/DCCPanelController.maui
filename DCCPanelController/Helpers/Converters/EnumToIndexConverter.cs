using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class EnumToIndexConverter<T> : IValueConverter where T : struct, Enum {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is T enumValue) {
            return Array.IndexOf(Enum.GetValues<T>(), enumValue);
        }
        return -1;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int index and >= 0) {
            var values = Enum.GetValues<T>();
            if (index < values.Length) {
                return (T)(values.GetValue(index) ?? default(T));
            }
        }
        return default(T); // Default value if not selected or invalid index
    }
}