namespace DCCPanelController.Clients;

public class DccClientMessage(string message, DccClientOperation operation = DccClientOperation.System, DccClientMessageType msgType = DccClientMessageType.System) {
    public string Message { get; init; } = message;
    public DccClientOperation Operation { get; init; } = operation;
    public DccClientMessageType MessageType { get; init; } = msgType;
    public DateTime TimeStamp { get; init; }= DateTime.Now;

    public string IconSource =>
        MessageType switch {
            DccClientMessageType.Inbound  => "message_in.png",
            DccClientMessageType.Outbound => "message_out.png",
            DccClientMessageType.System   => "message_system.png",
            DccClientMessageType.Error    => "message_error.png",
            _                             => "message_system.png"
        };

    public override string ToString() => $"{TimeStamp:HH:mm:ss.fff} {MessageType} {Operation}: {Message}";
}