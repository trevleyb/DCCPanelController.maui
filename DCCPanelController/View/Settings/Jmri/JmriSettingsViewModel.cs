using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri.Client;
using DCCCommon.Client;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Jmri;

public partial class JmriSettingsViewModel : SettingsViewModel {

    [ObservableProperty] private JmriClientSettings _jmriSettings;

    public JmriSettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) : base(settings, connectionService) {
        JmriSettings = Settings as JmriClientSettings ?? throw new InvalidOperationException();
    }

    private async Task InitializeAsync() {
        await OnRefreshServersClickedAsync();
    }

    [RelayCommand]
    private async Task OnDefaultDeviceNameClickedAsync() {
        JmriSettings.Name = DeviceInfo.Name;
    }

    public string Address {
        get => JmriSettings.Address;
        set {
            JmriSettings.Address = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IpAddress1));
            OnPropertyChanged(nameof(IpAddress2));
            OnPropertyChanged(nameof(IpAddress3));
            OnPropertyChanged(nameof(IpAddress4));
        }
    }

    public string IpAddress1 {
        get => GetIpAddressParts(1, JmriSettings.Address);
        set {
            var address = SetIpAddressParts(1, value, Address);
            JmriSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress2 {
        get => GetIpAddressParts(2, JmriSettings.Address);
        set {
            var address = SetIpAddressParts(2, value, Address);
            JmriSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress3 {
        get => GetIpAddressParts(3, JmriSettings.Address);
        set {
            var address = SetIpAddressParts(3, value, Address);
            JmriSettings.Address = address;
            OnPropertyChanged();
        }
    }

    public string IpAddress4 {
        get => GetIpAddressParts(4, JmriSettings.Address);
        set {
            var address = SetIpAddressParts(4, value, Address);
            JmriSettings.Address = address;
            OnPropertyChanged();
        }
    }

}