using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.WiThrottle.Client;
using DCCCommon.Client;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.WiThrottle;

public partial class WiThrottleSettingsViewModel: SettingsViewModel {
    
    [ObservableProperty] private WiThrottleClientSettings _wiThrottleSettings;

    public WiThrottleSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) {
        _wiThrottleSettings = _settings as WiThrottleClientSettings ?? throw new InvalidCastException("Invalid Client Settings type provided.");
    }
    
    [RelayCommand]
    private async Task OnDefaultDeviceNameClickedAsync() {
        WiThrottleSettings.Name = DeviceInfo.Name;
    }
    
    public string Address {
        get => WiThrottleSettings.Address;
        set {
            WiThrottleSettings.Address = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IpAddress1));
            OnPropertyChanged(nameof(IpAddress2));
            OnPropertyChanged(nameof(IpAddress3));
            OnPropertyChanged(nameof(IpAddress4));

        }
    }

    public string IpAddress1 {
        get => GetIpAddressParts(1, WiThrottleSettings.Address);
        set {
            var address = SetIpAddressParts(1, value, Address);
            WiThrottleSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress2 {
        get => GetIpAddressParts(2, WiThrottleSettings.Address);
        set {
            var address = SetIpAddressParts(2, value, Address);
            WiThrottleSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress3 {
        get => GetIpAddressParts(3, WiThrottleSettings.Address);
        set {
            var address = SetIpAddressParts(3, value, Address);
            WiThrottleSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress4 {
        get => GetIpAddressParts(4, WiThrottleSettings.Address);
        set {
            var address = SetIpAddressParts(4, value, Address);
            WiThrottleSettings.Address = address;
            OnPropertyChanged();
        }
    }
   
}