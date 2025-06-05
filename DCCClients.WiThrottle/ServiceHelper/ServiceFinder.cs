using System.Net;
using System.Net.Sockets;
using DccClients.WiThrottle.Client;
using Makaretu.Dns;

namespace DccClients.WiThrottle.ServiceHelper;

public static class ServiceFinder {
    /// <summary>
    ///     Uses MDNS to try and find services with the given name (such as WiThrottle) and then finds the
    ///     appropriate IPAddress for that service. It will return a collection of found services so that
    ///     one can be selected to connect to.
    /// </summary>
    /// <param name="serviceName">The name, or part name, to search for</param>
    /// <param name="timeout">How long should finding this take</param>
    /// <returns>A collection of Names and IPAddresses that match the service name</returns>
    public static async Task<List<ServiceInfo>> FindServices(string serviceName, int timeout = 2000) {
        List<ServiceInfo> foundServices = [];
        var sd = new ServiceDiscovery();

        sd.ServiceDiscovered += (s, domainName) => {
            Console.WriteLine($"ServiceFinder: Found service: {domainName}");
            if (CheckDomainNameComponents(domainName, serviceName)) {
                sd.QueryServiceInstances(domainName.ToString().Replace(".local", ""));
            }
        };

        sd.ServiceInstanceDiscovered += (s, e) => {
            Console.WriteLine($"ServiceFinder: Found instance: {e.ServiceInstanceName}");
            if (CheckDomainNameComponents(e.ServiceInstanceName, serviceName)) {
                var query = new Message();
                query.Questions.Add(new Question { Name = e.ServiceInstanceName, Type = DnsType.SRV });
                sd.Mdns.SendQuery(query);
            }
        };

        sd.Mdns.AnswerReceived += (s, e) => {
            foreach (var answer in e.Message.Answers) {
                Console.WriteLine($"ServiceFinder: Answer: {answer}");
                if (answer is SRVRecord srv) {
                    if (CheckDomainNameComponents(srv.Name, serviceName)) {
                        try {
                            var foundIps = Dns.GetHostAddresses(srv.Target.ToString());
                            foreach (var ip in foundIps) {
                                if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString() != "127.0.0.1" && !string.IsNullOrEmpty(ip.ToString())) {
                                    foundServices.Add(new ServiceInfo(srv.Name.ToString(), new WiThrottleClientSettings(ip.ToString(), srv.Port)));
                                }
                            }
                        } catch (Exception ex) {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        };

        sd.Mdns.Start();
        sd.QueryAllServices();
        try {
            //var cts = new CancellationTokenSource(timeout);
            //await Task.Delay(timeout, cts.Token);
        } catch (Exception ex) {
            Console.WriteLine(ex);
        } finally {
            sd.Mdns.Stop();
        }
        await Task.CompletedTask;
        return foundServices;
    }

    private static bool CheckDomainNameComponents(DomainName domainName, string serviceName) {
        return domainName.ToString().Contains(serviceName, StringComparison.OrdinalIgnoreCase) || domainName.Labels.Any(part => part.Contains(serviceName, StringComparison.OrdinalIgnoreCase));
    }
}