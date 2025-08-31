using System.Globalization;

namespace DCCPanelController.View.Converters;

public class BoolToColorConverter : IValueConverter {
    public Color TrueColor { get; set; } = Colors.Green;
    public Color FalseColor { get; set; } = Colors.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is bool isEnabled) {
            return isEnabled ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        // Do nothing. This converter is only for one-way binding.
        return Binding.DoNothing;
    }
}