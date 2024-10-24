using System.Globalization;
using DCCPanelController.Model;

namespace DCCPanelController.Helpers.Converters;

public class PanelToCardHeightConverter : IMultiValueConverter
{
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
        throw new NotImplementedException();
    }

}