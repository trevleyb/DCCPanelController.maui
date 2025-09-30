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
        var state = values[1] is DccClientState ? (DccClientState)values[1] : DccClientState.Connected;

        var baseName = state switch {
            DccClientState.Connected    => "wifi_on",
            DccClientState.Disconnected => "wifi_off",
            DccClientState.Error        => "wifi_error",
            DccClientState.Reconnecting => "wifi_reconnecting",
            DccClientState.Initialising => "wifi_initialising",
            _                            => "wifi_off",
        };

        // Add suffix based on Parameter 1
        var suffix = isActive ? "_active" : "_inactive";
        return$"{baseName}{suffix}.png";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}