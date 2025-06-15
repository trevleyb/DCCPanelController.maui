using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClient.Helpers;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Services;

public partial class ConnectionService : ObservableObject {

    private const int MaxServerMessages = 500;
    private readonly Profile _profile;

    public IDccClient? Client { get; private set; }
    public event EventHandler<bool>? ConnectStateChanged;

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    public ConnectionService(Profile profile) {
        _profile = profile ?? throw new NullReferenceException("Profile cannot be null.");
        _ = Task.Run(async () => await InitializeConnectionAsync());
    }

    private async Task InitializeConnectionAsync() {
        try {
            if (_profile.Settings.ConnectOnStartup) await ConnectAsync();
        } catch (Exception ex) {
            Console.WriteLine($"Initialisation connection error: {ex.Message}");
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
            if (_profile.Settings.ClientSettings is { } settings) {
                Client = DccClientFactory.CreateClient(_profile, _profile.Settings.ClientSettings);
                Client.ClientMessage += ClientOnClientMessage;
                if (Client is null) {
                    OnConnectionChanged(false);
                    return Result.Fail("Unable to create a Client instance.");
                }

                var connectResult = await Client.ConnectAsync();
                if (connectResult.IsFailure) {
                    OnConnectionChanged(false);
                    return Result.Fail("Unable to connect to the specified server.");
                }
                
                OnConnectionChanged(true);
                await SetTurnoutsToDefaultState();
                return connectResult;
            }
        } catch (Exception ex) {
            Console.WriteLine($"Connection Failed: {ex.Message}");
            OnConnectionChanged(false);
            return Result.Fail("Unable to connect to the server.", ex);
        }
        return Result.Fail("Unable to connect to the server.");
    }

    public async Task DisconnectAsync() {
        if (Client is { }) {
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
            _                         => false,
        };
        ConnectStateChanged?.Invoke(null, IsConnected);
        ServerMessages.Add(e.Message);
    }

    private void OnConnectionChanged(bool? isConnected = null) {
        if (isConnected is {}) IsConnected = isConnected.Value;
        Console.WriteLine($"ConnectionService: Connection State Changed to {IsConnected}");
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(Client));
        ConnectStateChanged?.Invoke(this,IsConnected);
    }
    
    public async Task SetTurnoutsToDefaultState() {
        if (_profile.Settings.SetTurnoutStatesOnStartup) {
            if (Client is { IsConnected: true }) {
                foreach (var turnout in _profile.Turnouts) {
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