using System.Globalization;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Converters;

public sealed class BoolToSpanConverter : IValueConverter
{
    // Defaults match the behavior we want:
    public int WhenTrue  { get; set; } = 1; // wide -> 1 column
    public int WhenFalse { get; set; } = 2; // narrow -> 2 columns

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? WhenTrue : WhenFalse;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class BoolToRowConverter : IValueConverter
{
    public int WhenTrue  { get; set; } = 0; // wide -> row 1
    public int WhenFalse { get; set; } = 1; // narrow -> row 2

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? WhenTrue : WhenFalse;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class BoolToRightColumnConverter : IValueConverter
{
    public int WhenTrue  { get; set; } = 1; // wide -> right column
    public int WhenFalse { get; set; } = 0; // narrow -> left column

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? WhenTrue : WhenFalse;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

