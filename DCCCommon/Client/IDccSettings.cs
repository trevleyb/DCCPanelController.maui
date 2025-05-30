namespace DCCCommon.Client;

public interface IDccSettings {
    string Type { get; set; }
    string Name { get; set; }
    string Address { get; set; }
    int Port { get; set; }
    string Url { get; }
}