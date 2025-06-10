using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriSignalEventArgs : JmriEventArgs<JmriSignalEventArgs>, IJmriEventArgs, IJmriProcessor<JmriSignalEventArgs> {
    public string Name { get; private init; } = string.Empty;
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

    public static bool HasChanged(JmriSignalEventArgs? existingItem, JmriSignalEventArgs newItem) {
        return existingItem == null || 
               existingItem.Appearance != newItem.Appearance ||
               existingItem.Lit != newItem.Lit || 
               existingItem.Held != newItem.Held;

    }

}