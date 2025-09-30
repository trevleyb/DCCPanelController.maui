using System.Globalization;
using DCCPanelController.Clients;

namespace DCCPanelController.View.Converters;

public class ConnectionIconConverter : IMultiValueConverter {
    public string ActiveIcon { get; set; } = "defaultx_active.png";
    public string InactiveIcon { get; set; } = "defaultx_inactive.png";

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values is not { Length: 2 }) return"defaultx.png";

        // Parameter 1: bool (determines _active or _inactive suffix)
        var isActive = values[0] as bool? ?? false;

        // Parameter 2: bool (determines which base name to use)
        var state = values[1] is DccClientStatus ? (DccClientStatus)values[1] : DccClientStatus.Connected;

        var baseName = state switch {
            DccClientStatus.Connected    => "wifi_on",
            DccClientStatus.Disconnected => "wifi_off",
            DccClientStatus.Error        => "wifi_error",
            DccClientStatus.Reconnecting => "wifi_reconnecting",
            DccClientStatus.Initialising => "wifi_initialising",
            _                            => "wifi_off",
        };

        // Add suffix based on Parameter 1
        var suffix = isActive ? "_active" : "_inactive";
        return$"{baseName}{suffix}.png";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}