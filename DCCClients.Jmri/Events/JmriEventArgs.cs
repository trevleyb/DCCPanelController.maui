using System.Collections.Concurrent;
using System.Text.Json;

namespace DccClients.Jmri.Events;

public abstract class JmriEventArgs<T> : EventArgs where T : class, IJmriEventArgs, IJmriProcessor<T> {
    public static bool ProcessMessage(JsonElement root,
                                      ConcurrentDictionary<string, T> collection,
                                      EventHandler<T>? eventHandler) {
        if (T.Create(root) is { } item) {
            var existingItem = collection.GetValueOrDefault(item.Name);
            collection[item.Name] = item;

            if (T.HasChanged(existingItem, item)) {
                eventHandler?.Invoke(null!, item); 
            }
            return true;
        }
        return false;
    }
}