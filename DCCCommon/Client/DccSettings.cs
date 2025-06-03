using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCCommon.Client;

public partial class DccSettings : ObservableObject, IDccSettings {

    [ObservableProperty] private string _name = "Unknown";
    [ObservableProperty] private string _type = "jmri";
    [ObservableProperty] private string _address = "localhost";
    [ObservableProperty] private int _port = 12080;
    [ObservableProperty] private string _protocol = "http";
    public string Url => $"{Protocol}://{Address}:{Port}";
}