using System.Net.WebSockets;

namespace DCCClients.Jmri.JMRI;

public class WebSocketWrapper : IWebSocket {
    private readonly ClientWebSocket _clientWebSocket;

    public WebSocketWrapper() {
        _clientWebSocket = new ClientWebSocket();
    }

    public WebSocketState State => _clientWebSocket.State;

    public Task ConnectAsync(Uri uri, CancellationToken token) {
        return _clientWebSocket.ConnectAsync(uri, token);
    }

    public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken token) {
        await _clientWebSocket.CloseAsync(closeStatus, statusDescription, token);
    }

    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken token) {
        return _clientWebSocket.ReceiveAsync(buffer, token);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token) {
        var result = _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, token);
        return result;
    }

    public void Dispose() {
        _clientWebSocket.Dispose();
    }
}