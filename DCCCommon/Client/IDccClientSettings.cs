using DCCCommon.Common;
using DCCCommon.Discovery;

namespace DCCCommon.Client;

public interface IDccClientSettings {
    DccClientType Type { get; }
    bool SetAutomatically { get; }
    bool SupportsManualEntries { get; }
}