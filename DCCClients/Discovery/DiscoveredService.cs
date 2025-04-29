using System.Collections.Generic;
using System.Net;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Represents a discovered network service
    /// </summary>
    public class DiscoveredService
    {
        /// <summary>
        /// The instance name of the service
        /// </summary>
        public string InstanceName { get; set; }
        
        /// <summary>
        /// The hostname of the service
        /// </summary>
        public string HostName { get; set; }
        
        /// <summary>
        /// The port number of the service
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// The service type (e.g., "_http._tcp.local")
        /// </summary>
        public string ServiceType { get; set; }
        
        /// <summary>
        /// IP addresses associated with the service
        /// </summary>
        public List<IPAddress> Addresses { get; init; } = [];
        
        /// <summary>
        /// TXT record values associated with the service
        /// </summary>
        public List<string> TxtRecords { get; set; } = [];
        
        public IPAddress Address => Addresses.Count > 0 ? Addresses[0] : new IPAddress(0);
        
        /// <summary>
        /// Gets the URL for this service
        /// </summary>
        /// <param name="preferIPv4">Whether to prefer IPv4 addresses over IPv6</param>
        /// <returns>The URL for this service</returns>
        public string GetUrl(bool preferIPv4 = true)
        {
            string hostAddress = HostName;
            
            // Use IP address if available
            if (Addresses.Count > 0)
            {
                if (preferIPv4)
                {
                    var ipv4 = Addresses.Find(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (ipv4 != null)
                    {
                        hostAddress = ipv4.ToString();
                    }
                    else
                    {
                        hostAddress = Addresses[0].ToString();
                    }
                }
                else
                {
                    hostAddress = Addresses[0].ToString();
                }
            }
            else
            {
                // Remove trailing dot from hostname if present
                if (hostAddress.EndsWith("."))
                {
                    hostAddress = hostAddress.Substring(0, hostAddress.Length - 1);
                }
            }
            
            // Determine protocol based on service type
            string protocol = ServiceType.Contains("_http") ? "http" : "tcp";
            
            return $"{protocol}://{hostAddress}:{Port}";
        }
    }
}
