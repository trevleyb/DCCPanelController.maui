using CommunityToolkit.Mvvm.Input;
using DccClients.WiThrottle.Client;
using DCCCommon.Client;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.WiThrottle;

public partial class WiThrottleSettingsViewModel: SettingsViewModel {
    new private readonly WiThrottleClientSettings _settings;
    
    public WiThrottleSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) {
        _settings = settings is WiThrottleClientSettings wiThrottle ? wiThrottle : new WiThrottleClientSettings();
    }

    [RelayCommand]
    private async Task OnDefaultDeviceNameClickedAsync() {
        _settings.Name = DeviceInfo.Name;
    }
    
    public string Name {
        get => _settings.Name;
        set {
            _settings.Name = value;
            OnPropertyChanged();
        }
    }

    public string Address {
        get => _settings.Address;
        set {
            _settings.Address = value;
            OnPropertyChanged();
        }
    }

    public int Port {
        get => _settings.Port;
        set {
            _settings.Port = value;
            OnPropertyChanged();
        }
    }

    public string IpAddress1 {
        get => GetIpAddressParts(1, _settings.Address);
        set => _settings.Address = SetIpAddressParts(1, value, _settings.Address);
    }

    public string IpAddress2 {
        get => GetIpAddressParts(2, _settings.Address);
        set => _settings.Address = SetIpAddressParts(2, value, _settings.Address);
    }

    public string IpAddress3 {
        get => GetIpAddressParts(3, _settings.Address);
        set => _settings.Address = SetIpAddressParts(3, value, _settings.Address);
    }

    public string IpAddress4 {
        get => GetIpAddressParts(4, _settings.Address);
        set => _settings.Address = SetIpAddressParts(4, value, _settings.Address);
    }
   
}