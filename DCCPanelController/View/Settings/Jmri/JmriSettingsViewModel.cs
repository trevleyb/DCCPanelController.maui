using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri.Client;
using DCCCommon.Client;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Jmri;

public partial class JmriSettingsViewModel : SettingsViewModel {

    [ObservableProperty] new private JmriClientSettings _settings;

    public JmriSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) {
        Settings = _settings as JmriClientSettings ?? throw new InvalidCastException("Invalid Client Settings type provided.");
    }

    private async Task InitializeAsync() {
        await OnRefreshServersClickedAsync();
    }

    [RelayCommand]
    private async Task OnDefaultDeviceNameClickedAsync() {
        Settings.Name = DeviceInfo.Name;
    }

    public bool ManualSettings => !Settings.SetAutomatically;
    
    public string Address {
        get => Settings.Address;
        set {
            Settings.Address = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IpAddress1));
            OnPropertyChanged(nameof(IpAddress2));
            OnPropertyChanged(nameof(IpAddress3));
            OnPropertyChanged(nameof(IpAddress4));
        }
    }

    public int Port {
        get => Settings.Port;
        set {
            Settings.Port = value;
            OnPropertyChanged();
        }
    }

    public string IpAddress1 {
        get => GetIpAddressParts(1, Settings.Address);
        set {
            var address = SetIpAddressParts(1, value, Address);
            Settings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress2 {
        get => GetIpAddressParts(2, Settings.Address);
        set {
            var address = SetIpAddressParts(2, value, Address);
            Settings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress3 {
        get => GetIpAddressParts(3, Settings.Address);
        set {
            var address = SetIpAddressParts(3, value, Address);
            Settings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress4 {
        get => GetIpAddressParts(4, Settings.Address);
        set {
            var address = SetIpAddressParts(4, value, Address);
            Settings.Address = address;
            OnPropertyChanged();
        }
    }

}