using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {

    /*
    private          Client?                       _client;
    private readonly Turnouts?                     _turnouts;
    public           ObservableCollection<Message> Messages { get; } = new();
    */  
    public SettingsPage() {
        InitializeComponent();
        /*
        _turnouts                  = MauiProgram.ServiceProvider.GetService<Turnouts>();
        BindingContext             = this;
        ServerName                 = "Click Find to search for a Withrottle service";
        Port.Text                  = "12090";
        Address.Text               = ServiceHelper.GetLocalIPAddress();
        ConnectButton.IsEnabled    = true;
        DisconnectButton.IsEnabled = false;
        */
    }
    
    /*
    private string _serverName = "Unknown";
    public string ServerName {
        get => _serverName;
        set {
            if (_serverName != value) {
                _serverName = value;
                OnPropertyChanged(nameof(ServerName));
            }
        }
    }

    private void FindButton_Clicked(object sender, EventArgs e) {
        var services = ServiceFinder.FindServices("withrottle");
        if (services.Count > 0) {
            ServerName   = services[0].Name;
            Address.Text = services[0].ClientInfo.Address;
            Port.Text    = services[0].ClientInfo.Port.ToString();
            OnPropertyChanged(nameof(Port));
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(ServerName));
        } else {
            ServerName = "No Withrottle service found";
            OnPropertyChanged(nameof(ServerName));
        }
    }

    private void ConnectButton_Clicked(object sender, EventArgs e) {
        TryConnect();
    }

    private void DisconnectButton_Clicked(object sender, EventArgs e) {
        TryDisconnect();
    }

    private void TryDisconnect() {
        if (_client == null) return;
        _client.Disconnect();
        _client.DataReceived       -= ClientOnDataReceived;
        _client.ConnectionError    -= ClientOnConnectionError;
        _client.MessageProcessed   -= ClientOnMessageProcessed;
        _client                    =  null;
        ConnectButton.IsEnabled    =  true;
        DisconnectButton.IsEnabled =  false;
    }

    private void TryConnect() {
        Messages.Clear();
        _turnouts?.Clear();
        var info = new ClientInfo(Address.Text, int.Parse(Port.Text));
        _client                 =  new Client(info, _turnouts);
        _client.DataReceived    += ClientOnDataReceived;
        _client.ConnectionError += ClientOnConnectionError;
        _client.MessageProcessed += ClientOnMessageProcessed;
        var result = _client.Connect();
        if (result.Failed) {
            Messages.Add(new Message {Text = "ERROR:"+result.Message });
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        } else {
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
        }
    }

    private void AddMessage(string message) {
        if (!string.IsNullOrEmpty(message)) {
            Messages.Insert(0, new Message { Text = message });
        }
    }
    
    private void ClientOnMessageProcessed(IClientMsg obj) {
        if (!string.IsNullOrEmpty(obj.ActionTaken)) {
            AddMessage(obj.ActionTaken);
        }
        OnPropertyChanged();
    }

    private void ClientOnConnectionError(string obj) {
        AddMessage("ERROR:"+obj);
    }

    private void ClientOnDataReceived(string obj) {
        //AddMessage("MSG:"+obj);
    }
    */
}

public sealed class Message : INotifyPropertyChanged
{
    private string   _text = string.Empty;
    private DateTime _time = DateTime.Now;

    public string Text {
        get => _text;
        set {
            if (_text != value) {
                _text = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime Time {
        get => _time;
        set {
            if (_time != value) {
                _time = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}