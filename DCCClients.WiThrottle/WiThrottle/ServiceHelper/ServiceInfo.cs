using System.Text.RegularExpressions;
using DCCClients.WiThrottle.Client;

namespace DCCClients.WiThrottle.ServiceHelper;

/// <summary>
///     Record containing the found information for a service
/// </summary>
public class ServiceInfo {
    public ServiceInfo(string name, string address, int port) : this(name, new WithrottleSettings(address, port)) { }

    public ServiceInfo(string name, WithrottleSettings withrottleSettings) {
        // Remove the trailing TCP part from the name of the service if it exists
        // Then make sure there are no Unicode identifiers in the string. If there are 
        // then remove then and replace with the correct ASCII character.
        // ---------------------------------------------------------------------------------
        if (!string.IsNullOrEmpty(name) && name.Contains("._")) {
            name = name.Substring(0, name.IndexOf("._", StringComparison.Ordinal));
        }

        Name = ConvertOctalToAscii(name);
        WithrottleSettings = withrottleSettings;
    }

    public string Name { get; set; }
    public WithrottleSettings WithrottleSettings { get; set; }

    private string ConvertOctalToAscii(string input) {
        if (string.IsNullOrEmpty(input)) return "";

        return Regex.Replace(input, @"\\(?<Decimal>[0-9]{1,3})", match => {
            var decimalNumber = match.Groups["Decimal"].Value;
            var number = int.Parse(decimalNumber);
            return ((char)number).ToString();
        });
    }
}