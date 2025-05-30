namespace DCCCommon.Client;

public class DccSettings : IDccSettings {
    public string Name { get; set; } = "Unknown";
    public string Type { get; set; } = "jmri";
    public string Address { get; set; } = "localhost";
    public int Port { get; set; } = 12080;
    public string Protocol { get; set; } = "http";
    public string Url => $"{Protocol}://{Address}:{Port}";
}