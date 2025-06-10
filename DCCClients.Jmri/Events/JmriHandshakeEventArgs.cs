using System.Text.Json;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriHandshakeEventArgs : EventArgs, IJmriEventArgs{
    public string Name => Railroad;
    public string JmriVersion { get; private set; } = string.Empty;
    public string JsonVersion { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Railroad { get; private set; } = string.Empty;
    public string Node { get; private set; } = string.Empty;
    public string Profile { get; private set; } = string.Empty;
    public int Heartbeat { get; private set; } = 0;

    private JmriHandshakeEventArgs() { }

    public static JmriHandshakeEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriHandshakeEventArgs() {
            JmriVersion = dataElement.GetStringProperty("JMRI"),
            JsonVersion = dataElement.GetStringProperty("json"),
            Version = dataElement.GetStringProperty("version"),
            Railroad = dataElement.GetStringProperty("railroad"),
            Node = dataElement.GetStringProperty("node"),
            Profile = dataElement.GetStringProperty("activeProfile"),
            Heartbeat = dataElement.GetIntProperty("heartbeat"),
        };
    }

    public static string? HelloMessage =>
        JsonSerializer.Serialize(new {
            type = "hello",
            client = "JMRI Layout Controller",
            version = "1.0",
        });
    
    public static string? GoodbyeMessage =>
        JsonSerializer.Serialize(new {
            type = "goodbye"
        });

    
}

/*
 
 If needed, these are the known available capabilities
 ----------------------------------------------------------
 
    throttle	Access locomotive throttles (speed, direction, functions)
    roster	    Read roster entries (locomotive info, decoder type)
    power	    Monitor and control track power
    sensor	    Receive sensor state updates
    turnout	    Monitor and control turnouts
    signalhead	Control signal head aspects
    light	    Control layout lighting
    consist	    Manage locomotive consists
    train	    Work with the dispatcher/train model
    layoutblock	Access layout blocks and block occupancy
    logixng	    Monitor or interact with LogixNG logic
    memory	    Access memory variables (JMRI internal state)
    cabSignal	Monitor or send cab signal data
    route	    Control JMRI-defined routes (macros of turnout/signal/etc.)
*/