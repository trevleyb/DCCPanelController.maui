using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients.Jmri.JMRI;
using DCCClients.WiThrottle.WiThrottle.Client;
using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using ConnectionInfo = DCCPanelController.Models.DataModel.ConnectionInfo;

namespace DCCPanelController.View;

public partial class SettingsViewModel : ConnectionViewModel {
    [ObservableProperty] private string _connectLabel = "Test Connection";
    [ObservableProperty] private bool   _isResetNameAvailable;
    [ObservableProperty] private bool   _isSearching;
    
    [ObservableProperty] private DiscoveredService? _selectedServer;
    [ObservableProperty] private ObservableCollection<DiscoveredService> _servers = [];
    [ObservableProperty] private ObservableCollection<SettingsMessage> _messages = [];

    [ObservableProperty] private Settings _settings;
    [ObservableProperty] private ConnectionInfo _connectionInfo;
    [ObservableProperty] private IDccSettings? _connectionSettings;
    
    //public Settings Settings => Profile.Settings;
    //public ConnectionInfo? CurrentSettings => Settings.ActiveConnection();

    public bool IsJmriServer { get; set; }
    public bool IsWiThrottle { get; set; }
    
    public SettingsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        if (ConnectionService.IsConnected) ConnectionService.DisconnectAsync();
        ConnectionService.ConnectionChanged += ConnectionServiceOnConnectionChanged;
        ConnectionService.ConnectionMessage += ClientOnMessageReceived;
        PropertyChanged += OnPropertyChanged;
        Settings = profile.Settings;
        ConnectionInfo = Settings.ActiveConnection;
        ConnectionSettings = ConnectionInfo.Settings;
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

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(ConnectionInfo.Name):
            IsResetNameAvailable = ConnectionInfo.Name != DeviceInfo.Name;
            break;
        case nameof(SelectedServer):
            if (SelectedServer is not null && ConnectionSettings is not null) {
                ConnectionSettings.Address = SelectedServer.Address.ToString();
                ConnectionSettings.Port = SelectedServer.Port;
            }
            break;
        case nameof(ConnectionSettings.Address):
            SetSelectedServer();
            break;
        case nameof(ConnectionSettings.Port):
            SetSelectedServer();
            break;
        }
    }

    public async Task SaveSettings() {
        await Profile.SaveAsync();
    }

    [RelayCommand]
    private async Task DefaultDeviceNameAsync() {
        if (ConnectionSettings is not null) {
            ConnectionSettings.Name = DeviceInfo.Name;
        }
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
            ConnectionSettings = new JmriSettings();
            ConnectionSettings.Port = 12080;
            ConnectionSettings.Type = "jmri";
            IsJmriServer = true;
            break;
        case "withrottle":
            ConnectionSettings = new WithrottleSettings();
            ConnectionSettings.Port = 12090;
            ConnectionSettings.Type = "withrottle";
            IsWiThrottle = true;
            break;
        };
    }
    
    private void SetSelectedServer() {
        SelectedServer = null;
        foreach (var server in Servers) {
            if (server.Address.ToString().Equals(ConnectionSettings?.Address) && server.Port.Equals(ConnectionSettings?.Port)) {
                SelectedServer = server;
                break;
            }
        }
    }
    
    [RelayCommand]
    private async Task ConnectAsync() {
        Messages.Clear();
        AddMessage("Attempting to connect/disconnect to Service");
        await SaveSettings();

        try {
            IsBusy = true;
            var result = await ConnectionService.ConnectAsync(ConnectionInfo);
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

        var serverType = ConnectionSettings?.Type;
        if (!string.IsNullOrEmpty(serverType)) {
            Messages.Clear();
            AddMessage("Attempting to scan for Available Servers");
            SelectedServer = null;

            try {
                IsSearching = true;
                IsBusy = true;
                var result = await DiscoverServices.SearchForServicesByTypeAsync(serverType);
                if (result is { IsSuccess: true, Value.Count: > 0 }) {
                    var servicesFound = result.Value.ToObservableCollection();
                    Servers = new ObservableCollection<DiscoveredService>(servicesFound);
                    AddMessage($"Found {Servers.Count} Server{(Servers.Count > 1 ? "s" : "")}");
                } else {
                    AddMessage($"{result.Message}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Unable to Refresh Servers: {ex.Message}");
            } finally {
                IsBusy = false;
                IsSearching = false;
                IsRefreshing = false;
                IsJmriServer = serverType.Equals("jmri");
                IsWiThrottle = serverType.Equals("withrottle");
                SetSelectedServer();
            }

            // If we only find a single server, set it as the one. 
            // -------------------------------------------------------------------
            if (Servers.Count == 1) {
                SelectedServer = Servers.FirstOrDefault();
                if (SelectedServer is { } && ConnectionSettings is { }) {
                    SetNewConnectionMethod(serverType);
                    ConnectionSettings.Address = SelectedServer.Address.ToString();
                    ConnectionSettings.Port = SelectedServer.Port;
                }
            }
        }
    }

    /// <summary>
    ///     Get the part of the IPAddress (1,2,3,4)
    /// </summary>
    /// <param name="part">The part number to get where 1= first, 2=second, 3=third, 4=fourth</param>
    /// <returns>The part of the Address</returns>
    private string GetIpAddressParts(int part) {
        if (string.IsNullOrEmpty(ConnectionSettings?.Address)) return "0";
        var parts = ConnectionSettings?.Address.Split('.');
        if (part == 0) part = 1;
        return parts?.Length >= part ? parts[part - 1] : "0";
    }

    /// <summary>
    ///     Sets the Address based on the constituent parts
    /// </summary>
    /// <param name="part">Get the part of the IPAddress (1,2,3,4)</param>
    /// <param name="value">the value to set</param>
    /// <returns>The full IPAddress</returns>
    private string SetIpAddressParts(int part, string value, [CallerMemberName] string? propertyName = null) {
        if (string.IsNullOrEmpty(value)) return ConnectionSettings?.Address ?? "0";
        var parts = ConnectionSettings?.Address.Split('.');

        if (ConnectionSettings is not null) {
            if (parts?.Length > 0) {
                if (part == 0) part = 1;
                if (parts?.Length >= part) parts[part - 1] = value;

                if (parts is not null) {
                    ConnectionSettings.Address = string.Join(".", parts);
                    OnPropertyChanged(propertyName);
                    OnPropertyChanged(nameof(ConnectionSettings.Address));
                }
                return ConnectionSettings.Address ?? "0.0.0.0";
            }
        }
        return "0";
    }
}

public partial class SettingsMessage(string message) : ObservableObject {
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}