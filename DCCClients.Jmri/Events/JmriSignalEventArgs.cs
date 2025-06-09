using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriSignalEventArgs : System.EventArgs {
    public string Name { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Appearance { get; private set; } = string.Empty;
    public bool Lit { get; private set; }
    public bool Held { get; private set; }

    private JmriSignalEventArgs() { }

    public static JmriSignalEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriSignalEventArgs() {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            Appearance = dataElement.GetStringProperty("appearance"),
            Lit = dataElement.GetBoolProperty("lit"),
            Held = dataElement.GetBoolProperty("held")
        };
    }
}