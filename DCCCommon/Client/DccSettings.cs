namespace DCCCommon.Client;

public class DccSettings : IDccSettings {
    public string Protocol { get; set; } = "http";

    public string Url {
        get => $"{Protocol}://{Address}:{Port}";
        set {
            if (string.IsNullOrEmpty(value)) {
                Protocol = "http";
                Address = "localhost";
                Port = 12080;
                return;
            }

            try {
                var uri = new Uri(value);
                Protocol = uri.Scheme;
                Address = uri.Host;
                Port = uri.Port;
            } catch (UriFormatException) {
                Protocol = "http";
                Address = "localhost";
                Port = 12080;
            }
        }
    }

    public string Name { get; set; } = "Unknown";
    public string Type { get; set; } = "jmri";
    public string Address { get; set; } = "localhost";
    public int Port { get; set; } = 12080;
}