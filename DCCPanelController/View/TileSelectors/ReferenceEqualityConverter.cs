using System.Globalization;

namespace DCCPanelController.View.TileSelectors;

public class RefEqualsConverter : IMultiValueConverter {
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture) {
        if (values == null || values.Length < 2) {
            return false;
        }

        var first = values[0];
        var second = values[1];
        return ReferenceEquals(first, second);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}