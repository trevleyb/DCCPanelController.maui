using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class SettingsViewModel : BaseViewModel {

    public Settings Settings { get; }
    private readonly SettingsService? _settingsService;
    
    [ObservableProperty] private ObservableCollection<WiServer> _wiServers = [];
    
    public SettingsViewModel(SettingsService settingsService) {
        Title = "Settings";
        _settingsService = settingsService;
        Settings = _settingsService.Settings;
    }

    public string IpAddress {
        get => Settings?.WiServer?.IpAddress ?? "";
        set {
            Settings.WiServer.IpAddress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IpAddress1));
            OnPropertyChanged(nameof(IpAddress2));
            OnPropertyChanged(nameof(IpAddress3));
            OnPropertyChanged(nameof(IpAddress4));
        }
    }
    
    public bool IsDemoMode {
        get => Settings?.DemoMode ?? false;
        set {
            Settings.DemoMode = value;
            OnPropertyChanged(nameof(IsDemoMode));
            OnPropertyChanged(nameof(IsLiveMode));
            OnPropertyChanged(nameof(IsConnectAvailable));
        }
    }

    public bool IsLiveMode => !IsDemoMode;
    public bool IsConnectAvailable => !IsBusy && !IsRefreshing && !IsDemoMode;
    
    public int Port {
        get => Settings?.WiServer?.Port ?? 12090;
        set {
            Settings.WiServer.Port = value;
            OnPropertyChanged();
        }
    }
    
    public string IpAddress1 {
        get => GetIpAddressParts(1);
        set => SetIpAddressParts(1, value);
    } 
    
    public string IpAddress2 {
        get => GetIpAddressParts(2);
        set => SetIpAddressParts(2, value);
    } 

    public string IpAddress3 {
        get => GetIpAddressParts(3);
        set => SetIpAddressParts(3, value);
    } 

    public string IpAddress4 {
        get => GetIpAddressParts(4);
        set => SetIpAddressParts(4, value);
    } 
    
    [RelayCommand]
    public void SaveSettings() {
        _settingsService?.Save();
    }

    [RelayCommand]
    public void LoadSettings() {
        _settingsService?.ReLoad();
    }

    [RelayCommand]
    public async Task RefreshWiServersAsync() {
        if (IsBusy) return;
        try {
            IsBusy = true;
            var servers = DCCWiThrottleClient.ServiceHelper.ServiceFinder.FindServices("withrottle",3000);
            WiServers.Clear();
            foreach (var server in servers) {
                WiServers.Add(new WiServer(server.Name, server.ClientInfo.Address, server.ClientInfo.Port));
            }
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unable to get Settings: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Settings States", ex.Message, "OK");
        }
        finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
 
    [RelayCommand]
    public async Task SelectWiServer(WiServer? server) {
        if (server == null) return;
        IpAddress = server.IpAddress;
        Port = server.Port;
    }

    /// <summary>
    /// Get the part of the IPAddress (1,2,3,4)
    /// </summary>
    /// <param name="part">The part number to get where 1= first, 2=second, 3=third, 4=fourth</param>
    /// <returns>The part of the Address</returns>
    private string GetIpAddressParts(int part) {
        if (Settings?.WiServer is { } wiServer) {
            wiServer.IpAddress ??= DCCWiThrottleClient.ServiceHelper.ServiceHelper.GetLocalIPAddress();
            var parts = wiServer.IpAddress.Split('.');
            if (part == 0) part = 1;
            return parts.Length >= part ? parts[part - 1] : "0";
        }
        return "0";
    }

    /// <summary>
    /// Sets the Address based on the constituent parts
    /// </summary>
    /// <param name="part">Get the part of the IPAddress (1,2,3,4)</param>
    /// <param name="value">the value to set</param>
    /// <returns>The full IPAddress</returns>
    private string SetIpAddressParts(int part, string value, [CallerMemberName] string? propertyName = null) {
        if (Settings?.WiServer?.IpAddress == null) return "0";
        if (string.IsNullOrEmpty(value)) return Settings.WiServer.IpAddress;
        var parts = Settings.WiServer?.IpAddress.Split('.');
        if (part == 0) part = 1;
        if (parts.Length >= part) parts[part-1] = value;
        if (Settings.WiServer != null) {
            Settings.WiServer.IpAddress = string.Join(".", parts);
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(IpAddress));
            return Settings.WiServer.IpAddress;
        }
        return "0";
    }
}