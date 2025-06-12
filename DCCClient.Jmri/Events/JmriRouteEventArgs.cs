using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriRouteEventArgs : JmriEventArgs<JmriRouteEventArgs>, IJmriEventArgs, IJmriProcessor<JmriRouteEventArgs> {
    public string Name { get; private init; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public int State { get; private set; } // 2=Active, 4=Inactive

    private JmriRouteEventArgs() { }

    public static JmriRouteEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriRouteEventArgs() {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            State = dataElement.GetIntProperty("state"),
            Comment = dataElement.GetStringProperty("comment")
        };
    }

    public static bool HasChanged(JmriRouteEventArgs? existingItem, JmriRouteEventArgs newItem) {
        return existingItem == null ||
               existingItem?.State != newItem.State;
    }
}