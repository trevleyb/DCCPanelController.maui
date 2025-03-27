using DCCClients.Interfaces;

namespace DCCClients;

public static class DccClientFactory {
    public static IDccClient Create(IDccSettings settings) {
        return settings.Type.ToLowerInvariant() switch {
            "withrottle" => new DccWiThrottleClient(settings),
            "jmri"       => new DccJmriClient(settings),
            _            => throw new NotImplementedException("Unknown client requested: {name}")
        };
    }
}