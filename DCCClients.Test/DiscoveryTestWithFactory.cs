using DCCCommon.Common;
using DCCCommon.Discovery;

namespace DCCClients.Test;

[TestFixture]
public class DiscoveryTestWithFactory {

    [Test]
    public async Task TestFindingJmriServices() {
        var result = await DiscoverServices.SearchForJmriServicesAsync();
        Assert.That(result.IsSuccess, Is.True);
        LogFoundServices(result);
    }
    
    [Test]
    public async Task TestFindingJmriServicesByType() {
        var result = await DiscoverServices.SearchForServicesByTypeAsync("jmri","jmri");
        Assert.That(result.IsSuccess, Is.True);
        LogFoundServices(result);
    }
    
    [Test]
    public async Task TestFindingWiThrottleServices() {
        var result = await DiscoverServices.SearchForWiThrottleServicesAsync();
        Assert.That(result.IsSuccess, Is.True);
        LogFoundServices(result);
    }
    
    [Test]
    public async Task TestFindingWiThrottleServicesByType() {
        var result = await DiscoverServices.SearchForServicesByTypeAsync("withrottle","jmri");
        Assert.That(result.IsSuccess, Is.True);
        LogFoundServices(result);
    }
 
    [Test]
    public async Task TestFindingHttpServicesByType() {
        var result = await DiscoverServices.SearchForServicesByTypeAsync("_http._tcp","jmri");
        Assert.That(result.IsSuccess, Is.True);
        LogFoundServices(result);
    }

    private static void LogFoundServices(IResult<List<DiscoveredService>> result) {
        if (result.Value is {Count: > 0} services) {
            foreach (var service in services) {
                Console.WriteLine($"Found Service: {service.FriendlyName} at {service.Address}:{service.Port}");
            }
        }
    }


}