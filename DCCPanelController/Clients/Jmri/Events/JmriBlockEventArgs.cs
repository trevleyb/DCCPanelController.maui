using System.Text.Json;
using DCCPanelController.Clients.Jmri.Helpers;

namespace DCCPanelController.Clients.Jmri.Events;

public class JmriBlockEventArgs : JmriEventArgs<JmriBlockEventArgs>, IJmriEventArgs, IJmriProcessor<JmriBlockEventArgs> {
    private JmriBlockEventArgs() { }
    public string UserName { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;      // Current value (like loco address)
    public string SensorName { get; private set; } = string.Empty; // Associated sensor
    public int State { get; private set; }                         // 2=Occupied, 4=Unoccupied
    public double Length { get; private set; }                     // Block length
    public bool Allocated { get; private set; }                    // Block allocation status
    public string Name { get; private set; } = string.Empty;

    public static JmriBlockEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriBlockEventArgs {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            State = dataElement.GetIntProperty("state"),
            Value = dataElement.GetStringProperty("value"),
            SensorName = dataElement.GetStringProperty("sensor"),
            Comment = dataElement.GetStringProperty("comment"),
            Length = dataElement.GetDoubleProperty("length"),
            Allocated = dataElement.GetBoolProperty("allocated")
        };
    }

    public static bool HasChanged(JmriBlockEventArgs? existingItem, JmriBlockEventArgs newItem) {
        return existingItem == null ||
               existingItem.State != newItem.State ||
               existingItem.Value != newItem.Value ||
               existingItem.Allocated != newItem.Allocated;
    }
}