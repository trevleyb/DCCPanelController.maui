using DCCClients.Common;
using DCCClients.Interfaces;
using DCCClients.JMRI;
using DCCClients.WiThrottle.Client;

namespace DCCClients;

public class DccInvalidClient : DccClient, IDccClient {
    private JmriSettings? _settings;

    public DccInvalidClient(IDccSettings? settings) {
        _settings = settings as JmriSettings;
    }

    /// <summary>
    ///     Establishes a connection to the WiThrottle server using the provided settings.
    /// </summary>
    /// <param name="settings">The settings required to configure the connection, such as server address and port.</param>
    /// <returns>Returns a result indicating the success or failure of the connection attempt.</returns>
    public async Task<IResult> ConnectAsync() {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    /// <summary>
    ///     Attempts to reconnect to the WiThrottle server using the existing client connection.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the reconnection attempt.</returns>
    public async Task<IResult> ReconnectAsync() {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    /// <summary>
    ///     Disconnects from the service and releases related resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnect operation.</returns>
    public IResult Disconnect() {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public bool IsConnected => false;

    public IResult SendCmd(string message) {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public IResult SendTurnoutCmd(string dccAddress, bool thrown) {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public IResult SendRouteCmd(string dccAddress, bool active) {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect) {
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }
}