using System.Globalization;
using DCCPanelController.Model;

namespace DCCPanelController.Helpers.Converters;

public class PanelToCardHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {

        if (values.Length == 2) {
            if (values[0] is Panel panel && values[1] is double width && panel is { Cols: > 0, Rows: > 0 } && width > 0) {
                var size = width / panel.Cols;      // Get the size of each grid. Grids are even
                var height = size * panel.Rows;
                return height;
            }
        }
        return 200; // Default height if Panel or cardWidth parameter is not available
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}