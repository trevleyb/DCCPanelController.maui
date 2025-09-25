
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Helpers;

public class TitleBarEnumConverter : IValueConverter {

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null && parameter != null && value.Equals(parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (bool)(value ?? Binding.DoNothing) ? parameter! : Binding.DoNothing;
}