using System.Runtime.CompilerServices;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

using System.ComponentModel;

public abstract class Tile : ContentView, ITile {
    public Entity Entity { get; init; }

    private const int DebounceDelay = 50;
    private bool _visualPropertiesChanged;
    private Dictionary<string, object?> _propertyCache = [];
    private CancellationTokenSource? _debounceRebuildCts;
    protected HashSet<string> VisualProperties { get; } = [];
    protected List<string> ChangedProperties = [];
    
    public double TileWidth => GridSize * Entity.Width;
    public double TileHeight => GridSize * Entity.Height;

    // @formatter:off
    public Microsoft.Maui.Controls.View? TileView { get; set => SetField(ref field, value); }
    public bool IsSelected {get; set => SetField(ref field, value); }
    public double GridSize {get; set => SetField(ref field, value); }
    // @formatter:on

    protected Tile(Entity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) {
        Entity = entity;
        GridSize = gridSize;
        PropertyChanged += OnPropertyChanged;
        entity.PropertyChanged += OnPropertyChanged;
        
        VisualProperties.Add(nameof(GridSize));
        VisualProperties.Add(nameof(Entity.Rotation));
        VisualProperties.Add(nameof(Entity.IsEnabled));
        VisualProperties.Add(nameof(Entity.Height));
        VisualProperties.Add(nameof(Entity.Width));
        VisualProperties.Add(nameof(Entity.Col));
        VisualProperties.Add(nameof(Entity.Row));
        SetContent();
    }

    protected abstract Microsoft.Maui.Controls.View? CreateTile();
    protected abstract Microsoft.Maui.Controls.View? CreateSymbol();
    private void SetContent() {
        BindingContext = this;
        Content = CreateTile();
        if (Content != null) {
            ClassId = Entity.Guid.ToString();
            SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), source: this));
            SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), source: this));
            SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), source: Entity));
            SetBinding(IsVisibleProperty, new Binding(nameof(Entity.IsEnabled), source: Entity));
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is { } property && VisualProperties.Contains(property)) {
            ChangedProperties.Add(property);
            _visualPropertiesChanged = true;
        }
        RebuildIfNecessary();
    }

    /// <summary>
    /// Rebuilds the tile's content if visual properties have changed,
    /// using debounce mechanisms to minimize redundant updates.
    /// </summary>
    /// <remarks>
    /// This method listens for changes in visual properties and schedules a delayed execution to rebuild the tile content if necessary.
    /// The debouncing mechanism ensures that multiple rapid changes are processed efficiently. The content is only updated if the
    /// visual properties have changed.
    /// </remarks>
    private void RebuildIfNecessary() {
        _debounceRebuildCts?.Cancel();
        _debounceRebuildCts = new CancellationTokenSource();
        var token = _debounceRebuildCts.Token;

        Task.Delay(DebounceDelay, token)
            .ContinueWith((t) => {
                 if (t.IsCanceled) return;
                 if (_visualPropertiesChanged) {
                     Console.WriteLine($"Rebuilding content for {Entity.Name} [{Entity.Guid}] due to: {string.Join(", ", ChangedProperties)}");
                     ChangedProperties.Clear();
                     Content = CreateTile();
                     _visualPropertiesChanged = false; // Reset flag
                 }
             }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Sets the value of a field and raises property-changing and changed notifications.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">The field to be updated.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property that changed. This parameter is optional and automatically supplied by the compiler.</param>
    /// <returns>
    /// True if the value of the field was changed, otherwise false.
    /// </returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public enum TileDisplayMode {
    Normal, Symbol
}