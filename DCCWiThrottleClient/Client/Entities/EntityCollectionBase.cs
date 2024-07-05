using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DCCWiThrottleClient.Client;

public abstract class EntityCollectionBase<T> : ObservableCollection<T> where T: INotifyPropertyChanged, IEntity, new() {
    
    public event Action<T> EntityChangedEvent;

    protected EntityCollectionBase() {
        CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (T item in e.NewItems) {
                item.PropertyChanged += OnPropertyChanged;
            }
        }

        if (e.OldItems != null) {
            foreach (T item in e.OldItems) {
                item.PropertyChanged -= OnPropertyChanged;
            }
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is T entity) {
            EntityChangedEvent?.Invoke(entity);
        }
    }

    public T Find(string name) {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        var entity = this.FirstOrDefault(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (entity == null) {
            entity = new T {
                Name = name,
                UserName = name
            };
            Add(entity);
        }
        return entity;
    }
}