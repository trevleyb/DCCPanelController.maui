using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DCCWiThrottleClient.ServiceHelper;

public static class ServiceHelper {
    
    public static string GetLocalIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString() != "127.0.0.1"){
                return ip.ToString();
            }
        }
        return "";
    }
    
    public static bool IsServiceRunningOnPort(int port) {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnections     = ipGlobalProperties.GetActiveTcpConnections();
        var listenersOnPort    = tcpConnections.Where(x => x.LocalEndPoint.Port.Equals(port));
        return listenersOnPort.Any();
    }
}