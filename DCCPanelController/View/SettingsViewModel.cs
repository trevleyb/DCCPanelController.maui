using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri.Client;
using DccClients.WiThrottle.Client;
using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class SettingsViewModel : ConnectionViewModel {
    [ObservableProperty] private bool _isJmriServer;
    [ObservableProperty] private bool _isWiThrottle;
    [ObservableProperty] private ObservableCollection<SettingsMessage> _messages = [];
   
    public SettingsViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        if (ConnectionService.IsConnected) ConnectionService.DisconnectAsync();
        ConnectionService.ConnectionChanged += ConnectionServiceOnConnectionChanged;
        ConnectionService.ConnectionMessage += ClientOnMessageReceived;
    }

    public Models.DataModel.Settings Settings => Profile.Settings;
    
    public Color BackgroundColor {
        get => Settings?.BackgroundColor ?? Colors.White;
        set {
            Settings.BackgroundColor = value;
            OnPropertyChanged();
        }
    }

    public async Task SaveSettings() {
        await Profile.SaveAsync();
    }
    
    // If the state of the Connect Changes, then we need to notify the UI that a change has occured. 
    // ---------------------------------------------------------------------------------------------
    private void ConnectionServiceOnConnectionChanged(object? sender, ConnectionChangedEvent e) {
        Console.WriteLine($"Connection Changed: {e.Status}");
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
}

public partial class SettingsMessage(string message) : ObservableObject {
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}