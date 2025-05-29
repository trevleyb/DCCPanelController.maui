using System.Net;
using System.Net.Sockets;

// Required for AddressFamily

namespace DCCCommon.Discovery;

public class DiscoveredService {
    public string InstanceName { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public IPAddress Address => Addresses.FirstOrDefault(n => n.AddressFamily == AddressFamily.InterNetwork) ?? new IPAddress(0);
    public List<IPAddress> Addresses { get; set; } = new();
    public Dictionary<string, string> TxtRecords { get; set; } = new();

    public string? GetUrl(bool preferIPv4 = true) {
        if (Port == 0) return null;

        string? targetAddress = null;

        if (Addresses.Any()) {
            IPAddress? selectedIp = null;
            if (preferIPv4) {
                selectedIp = Addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            }
            selectedIp ??= Addresses.FirstOrDefault(); // Fallback to any address

            if (selectedIp != null) {
                targetAddress = selectedIp.ToString();
            }
        }

        if (string.IsNullOrEmpty(targetAddress) && !string.IsNullOrEmpty(HostName)) {
            targetAddress = HostName.TrimEnd('.'); // Use hostname if no IP, might need DNS
        }

        if (string.IsNullOrEmpty(targetAddress)) return null;

        var protocol = "http"; // Default
        if (ServiceType.Contains("._tcp", StringComparison.OrdinalIgnoreCase)) {
            if (ServiceType.StartsWith("_https", StringComparison.OrdinalIgnoreCase) || ServiceType.Contains("._https", StringComparison.OrdinalIgnoreCase)) protocol = "https";
            else if (ServiceType.StartsWith("_ftp", StringComparison.OrdinalIgnoreCase) || ServiceType.Contains("._ftp", StringComparison.OrdinalIgnoreCase)) protocol = "ftp";

            // Add more specific protocol mappings if needed for other known services
        }

        // For generic TCP services, a "tcp://" scheme isn't standard for URLs.
        // The application layer protocol (like http, ftp, etc.) is what defines the scheme.

        return $"{protocol}://{targetAddress}:{Port}";
    }

    public override string ToString() {
        return $"Instance: {InstanceName}, Host: {HostName}, Port: {Port}, IPs: {string.Join(", ", Addresses.Select(a => a.ToString()))}, URL: {GetUrl() ?? "N/A"}";
    }
}