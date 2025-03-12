using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Helpers;

public class SelectedItemConverter : IMultiValueConverter {
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture) {
        if (values == null || values.Length != 2) return false;
        if (values[0] is null || values[1] is null ) return false;
        
        var currentPanel = values[0] as Panel;  // Current item in the CollectionView
        var selectedPanel = values[1] as Panel; // SelectedPanel from ViewModel
        return currentPanel == selectedPanel;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public object ConvertBack(object[]? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}