using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class PanelToCardHeightConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values.Length == 3) {
            if (values[0] is double width and > 0 && values[1] is int rows && values[2] is int cols) {
                var size = width / cols;
                var height = size * rows;
                return height;
            }
        }
        return 200; // Default height if Panel or cardWidth parameter is not available
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        // Ensure the value is a valid double (height in this case)
        if (value is double height and > 0) {
            // Ensure the parameter is provided (to preserve context about width, rows, or cols)
            if (parameter is object[] { Length: 2 } parameters) {
                var rows = (int)parameters[0];
                var cols = (int)parameters[1];

                // Reverse the height calculation
                var size = height / rows;
                var width = size * cols;

                return [width, rows, cols];
            }
        }
    
        // Default fallback if conversion could not be applied
        return [0.0, 0, 0]; // Default width, rows, and cols
    }
}