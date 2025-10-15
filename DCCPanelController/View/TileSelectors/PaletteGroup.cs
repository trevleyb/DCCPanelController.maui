using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces; 

namespace DCCPanelController.View.TileSelectors;

public sealed class Palette(Panel panel) : ObservableCollection<PaletteGroup>, INotifyPropertyChanged {
    public Panel Panel { get; init; } = panel;
}

public sealed class PaletteGroup : RangeObservableCollection<PaletteItem>, INotifyPropertyChanged {
    private readonly List<PaletteItem> _allItems = new();
    private readonly SemaphoreSlim     _gate     = new(1, 1);

    public string Category { get; }
    public bool IsExpanded { get; private set; }

    public PaletteGroup(string category) {
        Category = category;
        IsExpanded = true;
        ToggleExpandCommand = new AsyncRelayCommand(ToggleExpandedAsync);
    }

    public ICommand ToggleExpandCommand { get; }

    public void AddTile(ITile tile) {
        var item = new PaletteItem(tile, this);
        _allItems.Add(item);
        if (IsExpanded) base.InsertItem(Count, item); // visible immediately if expanded
    }

    private async Task ToggleExpandedAsync() {
        if (!await _gate.WaitAsync(0)) return; // coalesce fast double-taps
        try {
            IsExpanded = !IsExpanded;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));

            await MainThread.InvokeOnMainThreadAsync(() => {
                if (IsExpanded)
                    ReplaceAll(_allItems); // single Reset => no per-item inserts
                else
                    ClearWithReset(); // single Reset
            });
        } finally {
            _gate.Release();
        }
    }
}

public sealed partial class PaletteItem : ObservableObject {
    public PaletteItem(ITile tile, PaletteGroup group) {
        Tile = tile;
        IsSelected = false;
    }

    [ObservableProperty] private ITile _tile;
    [ObservableProperty] private bool  _isSelected;
}

public class RangeObservableCollection<T> : ObservableCollection<T> {
    private bool _suppressNotifications;

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        if (!_suppressNotifications)
            base.OnCollectionChanged(e);
    }

    /// <summary>Replaces the entire collection content and raises a single Reset.</summary>
    public void ReplaceAll(IEnumerable<T> items) {
        _suppressNotifications = true;
        try {
            base.ClearItems(); // mutate without spamming events
            foreach (var i in items)
                base.InsertItem(Count, i);
        } finally {
            _suppressNotifications = false;
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>Clears with a single Reset (no per-item removes).</summary>
    public void ClearWithReset() {
        _suppressNotifications = true;
        try {
            base.ClearItems();
        } finally {
            _suppressNotifications = false;
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}