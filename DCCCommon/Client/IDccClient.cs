using DCCCommon.Common;

namespace DCCCommon.Client;

public interface IDccClient : IDccClientCommands, IDccClientEvents {
    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
    Task<IResult> ReconnectAsync();
    Task<IResult> ForceRefreshAsync(string? type = null);
}