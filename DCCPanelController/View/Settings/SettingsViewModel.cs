using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCPanelController.Services;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Settings;

public partial class SettingsViewModel : Base.BaseViewModel {
    protected readonly IDccClientSettings Settings;
    protected readonly ConnectionService ConnectionService;

    [ObservableProperty] private string _connectLabel = "Test Connection";
    [ObservableProperty] private ObservableCollection<DiscoveredService> _servers = [];

    protected SettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) {
        Settings = settings;
        ConnectionService = connectionService;
    }

    [RelayCommand]
    protected async Task OnConnectClickedAsync() {
        ConnectionService.AddMessage($"Testing server connection: ({Settings.Type})","System",SystemMessageType.System);;
        try {
            IsBusy = true;
            var reconnect = ConnectionService.IsConnected;
            if (reconnect) await ConnectionService.DisconnectAsync();

            var result = await ConnectionService.ConnectAsync(Settings);
            if (result.IsFailure) {
                ConnectionService.AddMessage($"Unable to connect: {result.Message}","ERROR",SystemMessageType.Error);
                foreach (var error in result.Errors) ConnectionService.AddMessage(error.Message,"ERROR",SystemMessageType.Error);
                var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";
                await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
            } else {
                if (!reconnect) await ConnectionService.DisconnectAsync();
                ConnectionService.AddMessage("Connected Successfully.","System",SystemMessageType.System);
                await DisplayAlertHelper.DisplayOkAlertAsync("Connected", "Successfully connected to the server.");
            }
        } catch (Exception ex) {
            ConnectionService.AddMessage("Unable to Connect.","ERROR",SystemMessageType.Error);
            ConnectionService.AddMessage(ex.Message,"ERROR",SystemMessageType.Error);
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
            var result = await DiscoverServices.SearchForServicesByTypeAsync(Settings.Type);
            if (result is { IsSuccess: true, Value.Count: > 0 }) {
                var servicesFound = result.Value.ToObservableCollection();
                Servers = new ObservableCollection<DiscoveredService>(servicesFound);
                ConnectionService.AddMessage($"Found {Servers.Count} Server{(Servers.Count > 1 ? "s" : "")}","System",SystemMessageType.System);
            } else {
                ConnectionService.AddMessage($"{result.Message}","System",SystemMessageType.System);
            }
        } catch (Exception ex) {
            ConnectionService.AddMessage("Unable to Refresh Servers","ERROR",SystemMessageType.Error);
            ConnectionService.AddMessage(ex.Message,"ERROR",SystemMessageType.Error);
        } finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    protected string GetIpAddressParts(int part, string address) {
        if (string.IsNullOrEmpty(address)) return "0";
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
            if (parts is not null && parts.Length == 4) address = string.Join(".", parts);
        }
        return address ?? "0.0.0.0";
    }
}