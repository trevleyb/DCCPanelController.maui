using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCWithrottleClient.ServiceHelper;

namespace DCCPanelController.View;

public partial class SettingsViewModel : BaseViewModel {

    private readonly ConnectionService? _connectionService;
    public readonly SettingsService? SettingsService;

    [ObservableProperty] private ObservableCollection<WiServer> _wiServers = [];
    [ObservableProperty] private ObservableCollection<SettingsMessage> _messages = [];
    [ObservableProperty] private Settings _settings;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowWiServers))]
    private bool _showMessages;

    public SettingsViewModel(SettingsService settingsService) {
        SettingsService = settingsService;
        _connectionService = MauiProgram.ServiceHelper.GetService<ConnectionService>();
        if (_connectionService == null) IsDemoMode = true;
        if (_connectionService != null) _connectionService.PropertyChanged += ConnectionServiceOnPropertyChanged;
        Settings = SettingsService.Settings;
    }

    public string ConnectLabel => _connectionService is { IsConnected: true } ? "Disconnect" : "Connect";
    public bool ShowWiServers => !ShowMessages;
    public bool IsConnected => _connectionService is { IsConnected   : true } ? true : false;
    public bool IsLiveMode => !IsDemoMode || !IsConnected;
    public bool IsConnectAvailable => !IsBusy && !IsRefreshing && !IsDemoMode;

    public Color BackgroundColor {
        get => Settings?.BackgroundColor ?? Colors.White;
        set {
            Settings.BackgroundColor = value;
            OnPropertyChanged();
        }
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
        get => Settings?.UseConnection ?? false;
        set {
            Settings.UseConnection = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLiveMode));
            OnPropertyChanged(nameof(IsConnectAvailable));
        }
    }

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

    public void SaveSettings() {
        Console.WriteLine("Saving... in viewModel");
        SettingsService?.Save();
    }

    // If the state of the Connect Changes, then we need to notify the UI that a change has occured. 
    // ---------------------------------------------------------------------------------------------
    private void ConnectionServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(IsConnected):
            OnPropertyChanged(nameof(IsConnectAvailable));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectLabel));
            break;
        }
    }

    [RelayCommand]
    public async Task ConnectAsync() {
        if (!IsDemoMode) {
            Messages.Clear();
            AddMessage("Attempting to connect/disconnect to WiService");
            try {
                IsBusy = true;
                if (_connectionService is not null) {
                    if (IsConnected) {
                        _connectionService.Disconnect();
                        _connectionService.MessageRecieved -= ServiceOnMessageRecieved;
                        AddMessage("Disconnected.");
                    } else {
                        _connectionService.Connect(Settings.WiServer);
                        _connectionService.MessageRecieved += ServiceOnMessageRecieved;
                        AddMessage("Connected.");
                    }
                }
            } catch {
                IsBusy = false;
                AddMessage("Unable to Connect.");
            }
        }

        IsBusy = false;
    }

    private void ServiceOnMessageRecieved(string message) {
        AddMessage(message);
    }

    public void AddMessage(string message) {
        if (!string.IsNullOrEmpty(message)) {
            Messages.Add(new SettingsMessage(message));
            if (Messages.Count > 100) Messages.RemoveAt(0);
        }
    }

    [RelayCommand]
    public async Task ClearMessagesAsync() {
        Messages.Clear();
    }

    [RelayCommand]
    public async Task RefreshWiServersAsync() {
        if (IsBusy) return;
        AddMessage("Attempting to scan for WiThrottle Servers");
        try {
            IsBusy = true;
            OnPropertyChanged(nameof(IsConnectAvailable));
            WiServers.Clear();

            var servers = await ServiceFinder.FindServices("withrottle");
            if (servers is { Count: > 0 }) {
                foreach (var server in servers) {
                    WiServers.Add(new WiServer(server.Name, server.ClientInfo.Address, server.ClientInfo.Port));
                }
            }

            AddMessage($"Found {WiServers.Count} WiThrottle Servers");
        } catch (Exception ex) {
            AddMessage("Unable to search for WiThrottle Servers.");
            Debug.WriteLine($"Unable to get Settings: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Settings States", ex.Message, "OK");
        } finally {
            IsBusy = false;
            IsRefreshing = false;
            OnPropertyChanged(nameof(IsConnectAvailable));
        }
    }

    [RelayCommand]
    public void SelectWiServer(WiServer? server) {
        if (server == null) return;
        IpAddress = server.IpAddress;
        Port = server.Port;
    }

    /// <summary>
    ///     Get the part of the IPAddress (1,2,3,4)
    /// </summary>
    /// <param name="part">The part number to get where 1= first, 2=second, 3=third, 4=fourth</param>
    /// <returns>The part of the Address</returns>
    private string GetIpAddressParts(int part) {
        if (Settings?.WiServer is { } wiServer) {
            var parts = wiServer.IpAddress.Split('.');
            if (part == 0) part = 1;
            return parts.Length >= part ? parts[part - 1] : "0";
        }

        return "0";
    }

    /// <summary>
    ///     Sets the Address based on the constituent parts
    /// </summary>
    /// <param name="part">Get the part of the IPAddress (1,2,3,4)</param>
    /// <param name="value">the value to set</param>
    /// <returns>The full IPAddress</returns>
    private string SetIpAddressParts(int part, string value, [CallerMemberName] string? propertyName = null) {
        if (Settings?.WiServer is { } wiServer) {
            if (wiServer?.IpAddress == null) return "0";
            if (string.IsNullOrEmpty(value)) return Settings.WiServer.IpAddress;
            var parts = Settings.WiServer?.IpAddress.Split('.');
            if (parts?.Length > 0) {
                if (part == 0) part = 1;
                if (parts?.Length >= part) parts[part - 1] = value;

                if (parts is not null) {
                    wiServer.IpAddress = string.Join(".", parts);
                    OnPropertyChanged(propertyName);
                    OnPropertyChanged(nameof(IpAddress));
                }

                return Settings?.WiServer?.IpAddress ?? "0.0.0.0";
            }
        }

        return "0";
    }
}

public partial class SettingsMessage(string message) : ObservableObject {
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}