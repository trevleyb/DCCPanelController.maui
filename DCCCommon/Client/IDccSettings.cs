namespace DCCCommon.Client;

public interface IDccSettings {
    string Name { get; }
    string Type { get; }
    string Address { get; }
    int Port { get; }
}