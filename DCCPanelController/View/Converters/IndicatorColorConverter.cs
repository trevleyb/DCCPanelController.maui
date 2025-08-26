using System.Globalization;

namespace DCCPanelController.View.Converters;

public class IndicatorColorConverter: IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {

        // Dont ask: Had issues getting data to pass through as expected so 
        // mace this super generic to pass anything and get the correct color
        if (values is [int indicatorIndex, _, ..]) {
            int? currentIndex = null;
            if (values[1] is int) currentIndex = (int)values[1];
            if (values[1] is string) currentIndex = int.Parse((string)values[1]);
            if (values[1] is OperatePage { BindingContext: OperateViewModel vm }) currentIndex = vm.CurrentPanelIndex;
            if (values[1] is OperateViewModel vm2) currentIndex = vm2.CurrentPanelIndex;
            if (currentIndex is not null) {
                var active = indicatorIndex == currentIndex;
                return active
                    ? (Application.Current?.Resources["Primary"] as Color ?? Colors.Blue)
                    : Colors.DarkGray;
            }
        }
        return Colors.DarkGray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}