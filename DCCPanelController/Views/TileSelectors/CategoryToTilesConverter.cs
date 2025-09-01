using System.Collections.ObjectModel;
using System.Globalization;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Views.TileSelectors;

public class CategoryToTilesConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        var category = values.Length > 0 ? values[0] as string : null;
        var collection = values.Length > 1 ? values[1] : null;
        if (!string.IsNullOrEmpty(category) && collection is SideSelectorPanelViewModel vm) {
            var result = vm.ByCategory.TryGetValue(category, out var tiles) ? tiles : [];
            return result;
        }
        return new ObservableCollection<ITile>();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        return [];
    }
}