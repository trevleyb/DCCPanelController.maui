using DCCCommon.Common;
using DCCCommon.Events;

namespace DCCCommon;

public interface IDccClient : IDccClientCommands, IDccClientEvents {
    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
    Task<IResult> ReconnectAsync();
    Task<IResult> ForceRefreshAsync(string? type = null);
}
