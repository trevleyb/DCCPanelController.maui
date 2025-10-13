using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DCCPanelController.Clients;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {
    private readonly ProfileService _profileService;

    public ConnectionService ConnectionService { get; }
    public DccClientState ConnectionState => ConnectionService?.ConnectionState ?? DccClientState.Disconnected;
    private Profile Profile => _profileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile), "ConnectionViewModel: Active profile is not defined.");

    public bool IsConnectionAvailable => true;
    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    protected ConnectionViewModel(ProfileService profileService, ConnectionService connectionService) {
        _profileService = profileService;
        ConnectionService = connectionService;
        ConnectionService.ConnectionStateChanged += (sender, args) => {
            Console.WriteLine($"ConnectionViewModel: Connection State=> {args}");
            PropertiesChanged();
            IsConnected = ConnectionState == DccClientState.Connected;
        };

        WeakReferenceMessenger.Default.Register<ClientTypeChangedMessage>(this, (r, m) => {
            OnClientTypeChanged(m.Value);
        });

        ServerMessages = ConnectionService.ServerMessages;
        IsConnected = ConnectionState == DccClientState.Connected;
    }

    private void OnClientTypeChanged(DccClientType type) {
        MainThread.BeginInvokeOnMainThread(async void () => {
            try {
                await ConnectionService.DisconnectAsync();
                IsConnected = ConnectionState == DccClientState.Connected;
                PropertiesChanged();
            } catch (Exception e) {
                LogHelper.Logger.LogWarning($"Error in OnClientTypeChanged: {e.Message}");
            }
        });
    }

    public void PropertiesChanged() {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnectionAvailable));
        OnPropertyChanged(nameof(ConnectionState));
        OnPropertyChanged(nameof(ConnectionIcon));
        OnPropertyChanged(nameof(ConnectionIconPng));
        OnPropertyChanged(nameof(ConnectionText));
        OnPropertyChanged(nameof(ConnectionTextWithSystem));
        OnPropertyChanged(nameof(ConnectionColor));
    }
    
    public string ConnectionTextWithSystem => $"{ConnectionText} ({(Profile.Settings.ClientSettings?.Type.ToString() ?? "Unknown")})";
    public string ConnectionText =>
        ConnectionState switch {
            DccClientState.Connected    => "Connected",
            DccClientState.Disconnected => "Disconnected",
            DccClientState.Error        => "Error",
            DccClientState.Initialising => "Initialising",
            DccClientState.Reconnecting => "Reconnecting",
            _                           => "Unknown Connection State"
        };

    public Color ConnectionColor =>
        ConnectionState switch {
            DccClientState.Connected    => Colors.Green,
            DccClientState.Disconnected => Colors.Black,
            DccClientState.Error        => Colors.Red,
            DccClientState.Initialising => Colors.Blue,
            DccClientState.Reconnecting => Colors.DarkOrchid,
            _                           => Colors.Red
        };

    public string ConnectionIconPng => ConnectionIcon + ".png";
    public string ConnectionIcon =>
        ConnectionState switch {
            DccClientState.Connected    => "wifi_on",
            DccClientState.Disconnected => "wifi_off",
            DccClientState.Error        => "wifi_error",
            DccClientState.Reconnecting => "wifi_search",
            DccClientState.Initialising => "wifi_search",
            _                           => "wifi_off",
        };

    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        Console.WriteLine($"ConnectionViewModel: ToggleConnectionAsync");
        if (ConnectionState == DccClientState.Disconnected) {
            ConnectionService.ConnectionState = DccClientState.Initialising;
            OnPropertyChanged(nameof(ConnectionState));
        }

        var result = await ConnectionService.ToggleConnectionAsync();
        PropertiesChanged();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}