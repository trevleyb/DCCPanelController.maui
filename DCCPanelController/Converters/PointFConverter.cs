using System.Globalization;

namespace DCCPanelController.Converters;

public class PointFMultiConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values.Length == 2 && values[0] is double x && values[1] is double y) {
            return new Rect(x, y, AutoSize, AutoSize);
        }
        return new Rect(0, 0, AutoSize, AutoSize);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
    private static readonly double AutoSize = AbsoluteLayout.AutoSize;
}

public class PointFConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is double x && parameter is double y) {
            return $"{x}, {y}, Auto, Auto";
        }
        if (value is int a && parameter is int b) {
            return $"{a}, {b}, Auto, Auto";
        }
        return "0, 0, Auto, Auto";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }
}

public class PointCoordinateConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        // Value should come through as a x,y string value
        // -----------------------------------------------
        if (value is string bounds) {
            var coords = bounds.Split(',');
            if (coords.Length == 2) {
                var x = double.Parse(coords[0]);
                var y = double.Parse(coords[1]);
                return new Rect(x, y, AutoSize, AutoSize);
            }
        }
        return new Rect(0, 0, AutoSize, AutoSize);
    }

    private static readonly double AutoSize = AbsoluteLayout.AutoSize;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }
}