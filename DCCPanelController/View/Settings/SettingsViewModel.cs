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

    protected IDccClientSettings _settings;
    protected ConnectionService _connectionService;
  
    private void RaiseSettingsMessage(SettingsMessage message) => OnSettingsMessage?.Invoke(this, message);
    private void RaiseSettingsMessage(string message, bool clear = false) => OnSettingsMessage?.Invoke(this, new SettingsMessage(message, clear));

    public event EventHandler<SettingsMessage>? OnSettingsMessage;

    [ObservableProperty] private string _connectLabel = "Test Connection";
    [ObservableProperty] private ObservableCollection<DiscoveredService> _servers = [];
    
    protected SettingsViewModel(IDccClientSettings settings, ConnectionService connectionService) {
        _settings = settings;
        _connectionService = connectionService;
    }
    
    [RelayCommand]
    protected async Task OnConnectClickedAsync() {
        RaiseSettingsMessage($"Attempting to connect/disconnect to Service ({_settings.Type})", true);
        try {
            IsBusy = true;
            var result = await _connectionService.ConnectAsync(_settings);
            if (result.IsFailure) {
                RaiseSettingsMessage("Connection Failed.");
                foreach (var error in result.Errors) RaiseSettingsMessage(error.Message);
            } else {
                _connectionService.ConnectionMessage += ClientOnMessageReceived;
                await Task.Delay(1000);
                if (_connectionService.IsConnected) {
                    RaiseSettingsMessage("Connected Successfully.");
                    await _connectionService.DisconnectAsync();
                } else {
                    RaiseSettingsMessage("Connection Failed.");
                }
            }
        } catch {
            RaiseSettingsMessage("Unable to Connect.");
        } finally {
            _connectionService.ConnectionMessage -= ClientOnMessageReceived;
            _connectionService?.DisconnectAsync();
            IsBusy = false;
        }
        OnPropertyChanged(nameof(ConnectLabel));
        IsBusy = false;
    }

    private void ClientOnMessageReceived(object? sender, ConnectionMessageEvent e) {
        RaiseSettingsMessage(e.Message);
    }

    [RelayCommand] 
    protected async Task OnRefreshServersClickedAsync() {
        if (IsBusy) return;
        Servers.Clear();
        try {
            IsRefreshing = true;
            IsBusy = true;
            var result = await DiscoverServices.SearchForServicesByTypeAsync(_settings.Type);
            if (result is { IsSuccess: true, Value.Count: > 0 }) {
                var servicesFound = result.Value.ToObservableCollection();
                Servers = new ObservableCollection<DiscoveredService>(servicesFound);
                RaiseSettingsMessage($"Found {Servers.Count} Server{(Servers.Count > 1 ? "s" : "")}", true);
            } else {
                RaiseSettingsMessage($"{result.Message}", true);
            }
        } catch (Exception ex) {
            RaiseSettingsMessage("Unable to Refresh Servers", true);
            RaiseSettingsMessage(ex.Message);
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