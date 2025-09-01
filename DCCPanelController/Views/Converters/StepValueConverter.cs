using System.Globalization;

namespace DCCPanelController.Views.Converters;

public class StepValueConverter : IValueConverter
{
    public double Step { get; set; } = 1.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) { 
        if (value is double doubleValue) {
            return Math.Round(doubleValue / Step) * Step;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return Convert(value, targetType, parameter, culture);
    }
}