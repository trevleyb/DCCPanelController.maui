using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriLightEventArgs : JmriEventArgs<JmriLightEventArgs>, IJmriEventArgs, IJmriProcessor<JmriLightEventArgs> {
    public string Name { get; private init; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public int State { get; private set; } // 2=Active, 4=Inactive
    public bool Inverted { get; private set; }

    private JmriLightEventArgs() { }

    public static JmriLightEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriLightEventArgs() {
            //Name = dataElement.GetStringProperty("name"),
            //UserName = dataElement.GetStringProperty("userName"),
            //State = dataElement.GetIntProperty("state"),
            //Inverted = dataElement.GetBoolProperty("inverted")
        };
    }

    public static bool HasChanged(JmriLightEventArgs? existingItem, JmriLightEventArgs newItem) {
        return existingItem == null ||
               existingItem?.State != newItem.State;
    }
}