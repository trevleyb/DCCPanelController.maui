using System.Globalization;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Converters;

public class PanelToCardHeightConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture) {
        if (values is [double cardWidth, Panel panel, ..]) {
            // What is the ratio of the Columns to Rows
            // --------------------------------------------------------------
            if (cardWidth > 0) {
                var ratio = panel.Rows / (double)panel.Cols;
                var itemHeight = cardWidth * ratio;
                return itemHeight;
            }
        }
        return 150; // Fallback height
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}