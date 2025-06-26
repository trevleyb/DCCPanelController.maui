using System.Globalization;
using DCCPanelController;
using DCCPanelController.View;

public class IndicatorColorConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values?.Length >= 2 &&
            values[0] is int currentIndex &&
            values[1] is OperateViewModel viewModel) {
            try {
                return currentIndex == viewModel.CurrentPanelIndex ? Application.Current?.Resources["Primary"] as Color ?? Colors.Blue : Colors.DarkGray;
            } catch {
                return Colors.DarkGray;
            }
        }
        return Colors.DarkGray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}