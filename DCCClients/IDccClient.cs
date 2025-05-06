using DCCClients.Common;
using DCCClients.Events;

namespace DCCClients;

public interface IDccClient : IDccClientCommands, IDccClientEvents {
    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
}
