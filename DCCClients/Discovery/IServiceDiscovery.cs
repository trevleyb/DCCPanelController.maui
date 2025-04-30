namespace DCCClients.Discovery;

public interface IServiceDiscovery {    
    Task<List<DiscoveredService>> DiscoverServersAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default);
    Task<List<string>> DiscoverServerUrlsAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default);
}