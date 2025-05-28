using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.Jmri.JMRI;
using DCCClients.WiThrottle.WiThrottle.Client;
using DCCClients.WiThrottle.WiThrottle.ServiceHelper;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using ConnectionInfo = DCCPanelController.Models.DataModel.ConnectionInfo;

namespace DCCPanelController.View;

public partial class SettingsViewModel : ConnectionViewModel {
    public Settings Settings => Profile.Settings;
    public ConnectionInfo? CurrentSettings => Settings.ActiveConnection();

    [ObservableProperty] private string _name = "withrottle";
    [ObservableProperty] private string _ipAddress = "localhost";
    [ObservableProperty] private int _port = 12090;
    [ObservableProperty] private string _protocol = "http";
    [ObservableProperty] private string _url = "http://localhost:12090";
    [ObservableProperty] private string _connectLabel = "Test Connection";

    [ObservableProperty] private ObservableCollection<IDccSettings> _servers = [];
    [ObservableProperty] private ObservableCollection<SettingsMessage> _messages = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowWiServers))]
    private bool _showMessages;

    public bool ShowWiServers => !ShowMessages;

    public SettingsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        if (ConnectionService.IsConnected) ConnectionService.DisconnectAsync();
        ConnectionService.ConnectionChanged += ConnectionServiceOnConnectionChanged;
        ConnectionService.ConnectionMessage += ClientOnMessageReceived;

        Name = Profile.ActiveConnectionInfo?.Name ?? "default";
        if (Profile.ActiveConnectionInfo?.Settings is JmriSettings jmriSettings) {
            Name = jmriSettings.Name;
            IpAddress = jmriSettings.Address;
            Port = jmriSettings.Port;
            Protocol = jmriSettings.Protocol;
            Url = jmriSettings.Url;
        }

        if (Profile.ActiveConnectionInfo?.Settings is WithrottleSettings wiThrottleSettings) {
            Name = wiThrottleSettings.Name;
            IpAddress = wiThrottleSettings.Address;
            Port = wiThrottleSettings.Port;
            Protocol = wiThrottleSettings.Protocol;
            Url = wiThrottleSettings.Url;
        }
    }

    public Color BackgroundColor {
        get => Settings?.BackgroundColor ?? Colors.White;
        set {
            Settings.BackgroundColor = value;
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
        SaveConnectionDetails();
        Profile.SaveAsync();
    }

    // If the state of the Connect Changes, then we need to notify the UI that a change has occured. 
    // ---------------------------------------------------------------------------------------------
    private void ConnectionServiceOnConnectionChanged(object? sender, ConnectionChangedEvent e) {
        Console.WriteLine($"Connection Changed: {e.Status}");
        ConnectLabel = e.Status == ConnectionStatus.Connected ? "Testing" : "Test Connection";
        OnPropertyChanged(nameof(ConnectLabel));
    }

    public void SetNewConnectionMethod(string type) {
        switch (type) {
        case "jmri":
            Profile.ActiveConnectionInfo.Settings = new JmriSettings();
            SaveConnectionDetails();
            break;

        case "withrottle":
            Profile.ActiveConnectionInfo.Settings = new WithrottleSettings();
            SaveConnectionDetails();
            break;
        }
    }

    public void SaveConnectionDetails() {
        if (Profile?.ActiveConnectionInfo?.Settings is JmriSettings jmriSettings) {
            jmriSettings.Address = IpAddress;
            jmriSettings.Port = Port;
            jmriSettings.Protocol = Protocol;
            jmriSettings.Url = Url;
        }
        if (Profile?.ActiveConnectionInfo?.Settings is WithrottleSettings wiThrottleSettings) {
            wiThrottleSettings.Address = IpAddress;
            wiThrottleSettings.Port = Port;
            wiThrottleSettings.Protocol = Protocol;
            wiThrottleSettings.Url = Url;
        }
    }

    [RelayCommand]
    private async Task ConnectAsync() {
        Messages.Clear();
        AddMessage("Attempting to connect/disconnect to Service");

        try {
            IsBusy = true;
            SaveConnectionDetails();
            var result = await ConnectionService.ConnectAsync(Settings.ActiveConnection());
            if (result.IsFailure) {
                AddMessage("Connection Failed.");
                foreach (var error in result.Errors) AddMessage(error.Message);
            } else {
                ConnectionService.ConnectionMessage += ClientOnMessageReceived;
                await Task.Delay(1000);
                if (ConnectionService.IsConnected) {
                    AddMessage("Connected Successfully.");
                    await ConnectionService.DisconnectAsync();
                } else {
                    AddMessage("Connection Failed.");
                }
            }
        } catch {
            AddMessage("Unable to Connect.");
        } finally {
            ConnectionService.ConnectionMessage -= ClientOnMessageReceived;
            ConnectionService?.DisconnectAsync();
            IsBusy = false;
        }
        OnPropertyChanged(nameof(ConnectLabel));
        OnPropertyChanged(nameof(Messages));
        IsBusy = false;
    }

    private void ClientOnMessageReceived(object? sender, ConnectionMessageEvent e) {
        AddMessage($"{e.Message}");
    }

    public void AddMessage(string message) {
        if (!string.IsNullOrEmpty(message)) {
            var msg = new SettingsMessage(message);
            Messages.Add(msg);
            OnPropertyChanged(nameof(Messages));
            if (Messages.Count > 100) Messages.RemoveAt(0);
        }
    }

    [RelayCommand]
    public async Task ClearMessagesAsync() {
        Messages.Clear();
    }

    [RelayCommand]
    public async Task RefreshServersAsync() {
        if (IsBusy) return;
        AddMessage("Attempting to scan for Available Servers");
        Servers.Clear();
        OnPropertyChanged(nameof(Servers));

        try {
            IsBusy = true;
            var serverDiscovery = DCCClients.Discovery.ServiceDiscoveryFactory.DiscoverServices(CurrentSettings?.Settings?.Type ?? "");
            var servers = await serverDiscovery.DiscoverServersAsync();

            if (servers is { Count: > 0 }) {
                Console.WriteLine($"Found {servers.Count} Servers");
                foreach (var server in servers) {
                    Console.WriteLine($"Found Server: {server.HostName}");
                    Servers.Add(new DccSettings() {
                        Name = server.HostName,
                        Url = server.GetUrl(),
                        Type = CurrentSettings?.Settings?.Type ?? "unknown"
                    });
                }
            }
            AddMessage($"Found {Servers.Count} Servers");
        } catch (Exception ex) {
            AddMessage($"Unable to search for Servers of type {CurrentSettings?.Settings?.Type ?? "withrottle"}");
            Console.WriteLine($"Unable to get Settings: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Settings States", ex.Message, "OK");
        } finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public void SelectWiServer(WithrottleSettings? server) {
        if (server == null) return;
        IpAddress = server.Address;
        Port = server.Port;
    }

    /// <summary>
    ///     Get the part of the IPAddress (1,2,3,4)
    /// </summary>
    /// <param name="part">The part number to get where 1= first, 2=second, 3=third, 4=fourth</param>
    /// <returns>The part of the Address</returns>
    private string GetIpAddressParts(int part) {
        var parts = IpAddress.Split('.');
        if (part == 0) part = 1;
        return parts.Length >= part ? parts[part - 1] : "0";
    }

    /// <summary>
    ///     Sets the Address based on the constituent parts
    /// </summary>
    /// <param name="part">Get the part of the IPAddress (1,2,3,4)</param>
    /// <param name="value">the value to set</param>
    /// <returns>The full IPAddress</returns>
    private string SetIpAddressParts(int part, string value, [CallerMemberName] string? propertyName = null) {
        if (string.IsNullOrEmpty(value)) return IpAddress;
        var parts = IpAddress.Split('.');

        if (parts?.Length > 0) {
            if (part == 0) part = 1;
            if (parts?.Length >= part) parts[part - 1] = value;

            if (parts is not null) {
                IpAddress = string.Join(".", parts);
                OnPropertyChanged(propertyName);
                OnPropertyChanged(nameof(IpAddress));
            }

            return IpAddress ?? "0.0.0.0";
        }
        return "0";
    }
}

public partial class SettingsMessage(string message) : ObservableObject {
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}