using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace DCCPanelController.View.Converters;

public class BoolToIconConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var name = parameter?.ToString();
        var suffix = (value as bool?) == true ? "_active" : "_inactive";
        return $"{name}{suffix}.png";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class MultiBoolToIconConverter : IMultiValueConverter {
    
    public string ActiveIcon { get; set; } = "defaultx_active.png";
    public string InactiveIcon { get; set; } = "defaultx_inactive.png";
    
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        
        if (values is not { Length: 4 }) return "defaultx.png";

        // Parameter 1: bool (determines _active or _inactive suffix)
        var isActive = values[0] as bool? ?? false;

        // Parameter 2: bool (determines which base name to use)
        var useFirstName = values[1] as bool? ?? true;

        // Parameter 3: string (first base name option)
        var firstName = values[2]?.ToString() ?? "defaultx";

        // Parameter 4: string (second base name option)
        var secondName = values[3]?.ToString() ?? "defaultx";

        // Choose base name based on Parameter 2
        var baseName = useFirstName ? firstName : secondName;

        // Add suffix based on Parameter 1
        var suffix = isActive ? "_active" : "_inactive";

        return $"{baseName}{suffix}.png";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}