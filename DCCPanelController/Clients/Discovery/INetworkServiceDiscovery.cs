using DCCPanelController.Helpers;

namespace DCCPanelController.Clients.Discovery;

public interface INetworkServiceDiscovery : IDisposable {
    Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                 string subType,
                                                                 TimeSpan timeout,
                                                                 CancellationToken cancellationToken = default);

    Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                 TimeSpan timeout,
                                                                 CancellationToken cancellationToken = default);
}