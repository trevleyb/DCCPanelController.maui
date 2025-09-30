using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Discovery;
using DCCPanelController.Services;
using DCCPanelController.View.Base;

namespace DCCPanelController.View.Settings;

public partial class SettingsViewModel : BaseViewModel {
    protected readonly ConnectionService  ConnectionService;
    protected readonly IDccClientSettings Settings;

    [ObservableProperty] private string                                  _connectLabel = "Test Connection";
    [ObservableProperty] private ObservableCollection<DiscoveredService> _servers      = [];

    protected SettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) {
        Settings = settings;
        ConnectionService = connectionService;
    }

    [RelayCommand]
    protected async Task OnConnectClickedAsync() {
        ConnectionService.AddServerMessage($"Testing server connection: ({Settings.Type})");
        try {
            IsBusy = true;

            var reconnect = ConnectionService.ConnectionState;
            if (reconnect == DccClientStatus.Connected) await ConnectionService.DisconnectAsync();

            var result = await ConnectionService.ConnectAsync();
            if (result.IsFailure) {
                ConnectionService.AddServerMessage($"Unable to connect: {result.Message}", DccClientOperation.System, DccClientMessageType.Error);
                if (result?.Message is { } errMessage) ConnectionService.AddServerMessage(errMessage, DccClientOperation.System, DccClientMessageType.Error);
                if (result?.Exception is { } excMessage) ConnectionService.AddServerMessage(excMessage.Message, DccClientOperation.System, DccClientMessageType.Error);
                var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result?.Message) ? "." : $" due to {result?.Exception?.Message ?? "?"}")}";
                await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
            } else {
                if (reconnect != DccClientStatus.Connected) await ConnectionService.DisconnectAsync();
                ConnectionService.AddServerMessage("Connected Successfully.");
                await DisplayAlertHelper.DisplayOkAlertAsync("Connected", "Successfully connected to the server.");
            }
        } catch (Exception ex) {
            ConnectionService.AddServerMessage("Unable to Connect.", DccClientOperation.System, DccClientMessageType.Error);
            ConnectionService.AddServerMessage(ex.Message, DccClientOperation.System, DccClientMessageType.Error);
            var message = $"Unable to connect to the server due to {ex.Message}";
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        } finally {
            IsBusy = false;
        }
        IsBusy = false;
    }

    [RelayCommand]
    protected async Task OnRefreshServersClickedAsync() {
        if (IsBusy) return;
        Servers.Clear();
        try {
            IsRefreshing = true;
            IsBusy = true;
            var result = await DccClientFactory.FindServices(Settings.Type);
            if (result is { IsSuccess: true, Value.Count: > 0 }) {
                var servicesFound = result.Value.ToObservableCollection();
                Servers = new ObservableCollection<DiscoveredService>(servicesFound);
                ConnectionService.AddServerMessage($"Found {Servers.Count} Server{(Servers.Count > 1 ? "s" : "")}");
            } else {
                ConnectionService.AddServerMessage($"{result.Message}", DccClientOperation.System, DccClientMessageType.Error);
            }
        } catch (Exception ex) {
            ConnectionService.AddServerMessage("Unable to Refresh Servers", DccClientOperation.System, DccClientMessageType.Error);
            ConnectionService.AddServerMessage(ex.Message, DccClientOperation.System, DccClientMessageType.Error);
            var message = $"Unable to refresh servers due to {ex.Message}";
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Refreshing Servers", message);
        } finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    protected string GetIpAddressParts(int part, string address) {
        if (string.IsNullOrEmpty(address)) return"0";
        var parts = address.Split('.');
        if (part == 0) part = 1;
        var partStr = parts?.Length >= part ? parts[part - 1] : "0";
        return partStr;
    }

    protected string SetIpAddressParts(int part, string value, string address, [CallerMemberName] string? propertyName = null) {
        if (string.IsNullOrEmpty(value)) return address ?? "0";
        var parts = address.Split('.');
        if (parts?.Length > 0) {
            if (part == 0) part = 1;
            if (parts?.Length >= part) parts[part - 1] = value;
            if (parts is { } && parts.Length == 4) address = string.Join(".", parts);
        }
        return address ?? "0.0.0.0";
    }
}