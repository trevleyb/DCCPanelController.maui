using System.Globalization;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.View.Converters;

public class SelectedBorderConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        // 'value' is the current item's BackgroundColor.
        // 'parameter' is the SelectedColor passed from the ConverterParameter.
        if (value == null || parameter == null)
            return Colors.Transparent;

        // Compare the current item's color with the SelectedColor.
        var currentItemColor = (Color)value;
        var selectedColor = (Color)parameter;

        return Equals(currentItemColor, selectedColor) ? Colors.Black : Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == null || parameter == null) return Colors.Transparent;

        var borderColor = (Color)value;
        var selectedColor = (Color)parameter;

        // If the border is Black, it represents the selected state, so return the SelectedColor.
        return Equals(borderColor, Colors.Black) ? selectedColor : Colors.Transparent;
    }
}