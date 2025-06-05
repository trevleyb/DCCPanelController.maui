using System.Net.WebSockets;

namespace DccClients.Jmri.Client;

public interface IWebSocket : IDisposable {
    WebSocketState State { get; }
    Task ConnectAsync(Uri uri, CancellationToken token);
    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken token);
    ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken token);
    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token);
}