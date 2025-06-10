using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriTurnoutEventArgs : JmriEventArgs<JmriTurnoutEventArgs>, IJmriEventArgs, IJmriProcessor<JmriTurnoutEventArgs> {
    public string Name { get; private init; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public int State { get; private set; } // 2=Closed, 4=Thrown
    public bool Inverted { get; private set; }

    private JmriTurnoutEventArgs() { }

    public static JmriTurnoutEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriTurnoutEventArgs {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            State = dataElement.GetIntProperty("state"),
            Comment = dataElement.GetStringProperty("comment"),
            Inverted = dataElement.GetBoolProperty("inverted")
        };
    }

    public static bool HasChanged(JmriTurnoutEventArgs? existingItem, JmriTurnoutEventArgs newItem) {
        return existingItem == null || 
               existingItem?.State != newItem.State;
    }
} 