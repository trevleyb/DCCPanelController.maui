using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DCCClients.Common;

namespace DCCClients.Discovery;

public interface INetworkServiceDiscovery : IDisposable {
    Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                 string subType,
                                                                 TimeSpan timeout,
                                                                 CancellationToken cancellationToken = default);

    Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                 TimeSpan timeout,
                                                                 CancellationToken cancellationToken = default);

}