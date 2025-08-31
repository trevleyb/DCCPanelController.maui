using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClient.Helpers;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Services;

public partial class ConnectionService : ObservableObject {
    private const int MaxServerMessages = 500;

    private readonly ILogger<ConnectionService> _logger = LogHelper.CreateLogger<ConnectionService>();
    private readonly ProfileService.ProfileService _profileService;

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    public ConnectionService(ProfileService.ProfileService profileService) {
        _profileService = profileService;
        _ = Task.Run(async () => await InitializeConnectionAsync());
    }

    public IDccClient? Client { get; private set; }

    public static ConnectionService Instance => MauiProgram.ServiceHelper.GetService<ConnectionService>();
    public event EventHandler<bool>? ConnectStateChanged;

    private async Task InitializeConnectionAsync() {
        try {
            if (_profileService?.ActiveProfile?.Settings.ConnectOnStartup ?? false) await ConnectAsync();
            _logger.LogInformation("Initialised Connection");
        } catch (Exception ex) {
            _logger.LogError("Initialisation connection error: {Message}", ex.Message);
        }
    }

    public async Task<IResult> ToggleConnectionAsync() {
        if (IsConnected) {
            await DisconnectAsync();
            return Result.Ok();
        }
        return await ConnectAsync();
    }

    public async Task<IResult> ConnectAsync() {
        try {
            if (Client is { IsConnected : true }) await DisconnectAsync();
            var activeProfile = _profileService.ActiveProfile;
            if (activeProfile?.Settings.ClientSettings is { } settings) {
                Client = DccClientFactory.CreateClient(activeProfile, activeProfile.Settings.ClientSettings);
                Client.ClientMessage += ClientOnClientMessage;
                if (Client is null) {
                    OnConnectionChanged(false);
                    _logger.LogDebug("Unable to create a Client instance.");
                    return Result.Fail("Unable to create a Client instance.");
                }

                var connectResult = await Client.ConnectAsync();
                if (connectResult.IsFailure) {
                    OnConnectionChanged(false);
                    _logger.LogDebug("Unable to connect to the specified server.");
                    return Result.Fail("Unable to connect to the specified server.");
                }

                OnConnectionChanged(true);
                await SetTurnoutsToDefaultState();
                await ResetOccupancyStates();
                return connectResult;
            }
        } catch (Exception ex) {
            _logger.LogDebug("Connection Failed: {Message}", ex.Message);
            OnConnectionChanged(false);
            return Result.Fail("Unable to connect to the server.", ex);
        }
        _logger.LogDebug("Unable to connect to the specified server.");
        return Result.Fail("Unable to connect to the server.");
    }

    public async Task DisconnectAsync() {
        if (Client is not null) {
            await ResetOccupancyStates();
            if (Client.IsConnected) await Client.DisconnectAsync();
            Client.ClientMessage -= ClientOnClientMessage;
            Client = null;
        }
        OnConnectionChanged(false);
    }

    private void ClientOnClientMessage(object? sender, DccClientEvent e) {
        if (e.Message is null) return;
        IsConnected = e.Status switch {
            DccClientStatus.Connected => true,
            _                         => false
        };
        ConnectStateChanged?.Invoke(null, IsConnected);
        ServerMessages.Add(e.Message);
    }

    private void OnConnectionChanged(bool? isConnected = null) {
        if (isConnected is not null) IsConnected = isConnected.Value;
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(Client));
        ConnectStateChanged?.Invoke(this, IsConnected);
    }

    public async Task ResetOccupancyStates() {
        var profile = _profileService.ActiveProfile;
        if (profile is not null) {
            foreach (var sensor in profile.Sensors) sensor.State = false;
        }
    }

    public async Task SetTurnoutsToDefaultState() {
        var profile = _profileService.ActiveProfile;
        if (profile?.Settings?.SetTurnoutStatesOnStartup ?? false) {
            if (Client is { IsConnected: true }) {
                foreach (var turnout in profile.Turnouts) {
                    if (turnout.Default != TurnoutStateEnum.Unknown && !string.IsNullOrEmpty(turnout?.Id)) {
                        await Client.SendTurnoutCmdAsync(turnout, turnout.State == TurnoutStateEnum.Thrown)!;
                    }
                }
            }
        }
    }

    public virtual void AddServerMessage(string message, DccClientOperation operation = DccClientOperation.System, DccClientMessageType msgType = DccClientMessageType.System) {
        AddServerMessage(new DccClientMessage(message, operation, msgType));
    }

    public virtual void AddServerMessage(DccClientMessage message) {
        ServerMessages.Add(message);
    }

    [RelayCommand]
    public void ClearMessages() {
        ServerMessages.Clear();
        OnPropertyChanged(nameof(ServerMessages));
    }
}