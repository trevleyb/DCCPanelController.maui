using DCCCommon.Common;

namespace DCCCommon.Client;

public class DccInvalidClient : DccClient, IDccClient {
    private IDccSettings? _settings;

    public DccInvalidClient(IDccSettings? settings) {
        _settings = settings;
    }

    /// <summary>
    ///     Establishes a connection to the WiThrottle server using the provided settings.
    /// </summary>
    /// <param name="settings">The settings required to configure the connection, such as server address and port.</param>
    /// <returns>Returns a result indicating the success or failure of the connection attempt.</returns>
    public async Task<IResult> ConnectAsync() {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    /// <summary>
    ///     Attempts to reconnect to the WiThrottle server using the existing client connection.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the reconnection attempt.</returns>
    public async Task<IResult> ReconnectAsync() {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> ForceRefreshAsync(string? type = null) {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> TestConnection() {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    /// <summary>
    ///     Disconnects from the service and releases related resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnect operation.</returns>
    public async Task<IResult> DisconnectAsync() {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public bool IsConnected => false;

    public async Task<IResult> SendCmdAsync(string message) {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> SendTurnoutCmdAsync(string dccAddress, bool thrown) {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> SendRouteCmdAsync(string dccAddress, bool active) {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> SendSignalCmdAsync(string dccAddress, SignalAspectEnum aspect) {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }

    public async Task<IResult> ForceRefreshAsync() {
        await Task.CompletedTask;
        return Result.Fail(new Error("Invalid Client not currently implemented"));
    }
}