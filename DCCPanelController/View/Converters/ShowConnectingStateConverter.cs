using System.Globalization;
using DCCPanelController.Clients;

namespace DCCPanelController.View.Converters;

public class ShowConnectingStateConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is DccClientState state) {
            return state switch {
                DccClientState.Connected    => false,
                DccClientState.Disconnected => false,
                DccClientState.Initialising => true,
                DccClientState.Reconnecting => true,
                DccClientState.Error        => false,
                _                           => false,
            };
        }
        return false;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ((IValueConverter)this).ConvertBack(value, targetType, parameter, culture);
}

public class ShowNotConnectingStateConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is DccClientState state) {
            return state switch {
                DccClientState.Connected    => false,
                DccClientState.Disconnected => true,
                DccClientState.Initialising => false,
                DccClientState.Reconnecting => false,
                DccClientState.Error        => false,
                _                           => false,
            };
        }
        return false;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ((IValueConverter)this).ConvertBack(value, targetType, parameter, culture);
}