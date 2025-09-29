using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using MethodTimer;

namespace DCCPanelController.Models.ViewModel.Tiles;

[DebuggerDisplay("{Entity.EntityName} @ {Entity.Col},{Entity.Row}")]
public abstract class Tile : ContentView, ITile, IDisposable {
    // @formatter:off
    public Entity   Entity          { get; init; }
    public bool     IsDesignMode    { get; set;} = false;
    public bool     IsSelected      { get; set => SetField(ref field, value); }
    public double   GridSize        { get; set => SetField(ref field, value); }
    
    public double   TileWidth => GridSize * Entity.Width;
    public double   TileHeight => GridSize * Entity.Height;
    // @formatter:on

    protected const float DefaultScaleFactor = 1.5f;
    protected const float SymbolScaleFactor  = 0.75f;
    private const   int   DebounceDelay      = 75;

    private int                      _rebuildGuard; // >0 means "we're rebuilding, ignore change events"
    private CancellationTokenSource? _debounceRebuildCts;

    protected readonly PropertyChangeTracker Watch = new();
    protected bool UseClickSounds => Entity?.Parent?.Panels?.Profile?.Settings?.UseClickSounds ?? true;

    protected bool Disposed;
    protected bool HaveDimensionsChanged;
    protected bool HaveVisualPropertiesChanged;

    public event EventHandler<TileChangedEventArgs>? TileChanged;

    protected virtual void OnTileChanged(TileChangeType changeType = TileChangeType.Modified) => TileChanged?.Invoke(this, new TileChangedEventArgs(this, changeType));
    protected virtual void OnTileChanged(string propertyName, object? oldValue, object? newValue) => TileChanged?.Invoke(this, new TileChangedEventArgs(this, propertyName, oldValue, newValue));

    protected Tile(Entity entity, double gridSize) {
        Entity = entity;
        GridSize = gridSize;

        PropertyChanged += OnTilePropertyChanged;
        entity.PropertyChanged += OnTilePropertyChanged;

        // Track only values that impact the visual tree/content
        Watch
           .Track(nameof(GridSize),        () => GridSize,            Tolerance.Double(0.01))  // 1% grid change threshold
           .Track(nameof(Entity.Col),      () => Entity.Col)
           .Track(nameof(Entity.Row),      () => Entity.Row)
           .Track(nameof(Entity.Width),    () => Entity.Width)
           .Track(nameof(Entity.Height),   () => Entity.Height)
           .Track(nameof(Entity.Layer),    () => Entity.Layer)
           .Track(nameof(Entity.Rotation), () => Entity.Rotation,      Tolerance.Double(0.1))   // 0.1° tolerance
           .Track(nameof(Entity.Opacity),  () => Entity.Opacity,       Tolerance.Double(0.01)); // 1% opacity ;

        SetContent();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this); 
    }

    protected void Dispose(bool disposing) {
        if (!Disposed && disposing) {
            Cleanup();
            Disposed = true;
        }
    }

    protected virtual void Cleanup() {
        Entity.PropertyChanged -= OnTilePropertyChanged;
        PropertyChanged -= OnTilePropertyChanged;
        _debounceRebuildCts?.Cancel();
        _debounceRebuildCts = null;
    }

    protected abstract Microsoft.Maui.Controls.View? CreateTile();

    public void ForceRedraw([CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0) {
        //Console.WriteLine($"Force Redraw Called: {memberName}@{sourceLineNumber}");
        HaveDimensionsChanged = false;
        HaveVisualPropertiesChanged = true;
        RebuildIfNecessary();
    }
    
    private void SetContent([CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0) {
        try {
            Interlocked.Increment(ref _rebuildGuard);
            PropertyChanged -= OnTilePropertyChanged;
            Entity.PropertyChanged -= OnTilePropertyChanged;

            BindingContext = this;
            Content = CreateTile();
            if (Content != null) {
                Content.ClassId = Entity.Guid.ToString();
                Content.SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), BindingMode.OneWay, source: this));
                Content.SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), BindingMode.OneWay, source: this));
                Content.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), BindingMode.OneWay, source: Entity));
                Content.SetBinding(IsVisibleProperty, new Binding(nameof(Entity.IsEnabled), BindingMode.OneWay, source: Entity));
            }
        } finally {
            PropertyChanged += OnTilePropertyChanged;
            Entity.PropertyChanged += OnTilePropertyChanged;
            Interlocked.Decrement(ref _rebuildGuard);
        }
    }

    protected void OnTilePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (_rebuildGuard > 0) return; // keep this guard you already have
        if (string.IsNullOrEmpty(e.PropertyName)) return;

        // Only consider properties we track AND only if the value truly changed.
        if (!Watch.IsTracked(e.PropertyName) || !Watch.HasChanged(e.PropertyName)) return;

        // Dimension changes (affects measure) vs. visual-only
        HaveDimensionsChanged = e.PropertyName is nameof(Entity.Col)
            or nameof(Entity.Row)
            or nameof(Entity.Width)
            or nameof(Entity.Height)
            or nameof(Entity.Rotation);
        HaveVisualPropertiesChanged = true;

        Debug.WriteLine($"{Entity.EntityName} => visual change → {e.PropertyName}");
        RebuildIfNecessary();
    }


    /// <summary>
    ///     Rebuilds the tile's content if visual properties have changed,
    ///     using debounce mechanisms to minimize redundant updates.
    /// </summary>
    /// <remarks>
    ///     This method listens for changes in visual properties and schedules a delayed execution to rebuild the tile content
    ///     if necessary.
    ///     The debouncing mechanism ensures that multiple rapid changes are processed efficiently. The content is only updated
    ///     if the visual properties have changed.
    /// </remarks>
    private void RebuildIfNecessary() {
        _debounceRebuildCts?.Cancel();
        _debounceRebuildCts?.Dispose();

        _debounceRebuildCts = new CancellationTokenSource();
        var token = _debounceRebuildCts.Token;

        Task.Delay(DebounceDelay, token)
            .ContinueWith(async t => {
                 if (t.IsCanceled) return;
                 try {
                     await MainThread.InvokeOnMainThreadAsync(() => {
                         if (HaveVisualPropertiesChanged) {
                             SetContent();
                             OnTileChanged(TileChangeType.Visual);
                         }
                     });
                     if (HaveDimensionsChanged) OnTileChanged(TileChangeType.Dimensions);
                     HaveDimensionsChanged = false;       // Reset flag
                     HaveVisualPropertiesChanged = false; // Reset flag
                 } catch (Exception ex) {
                     Debug.WriteLine($"Error rebuilding tile: {ex.Message}");
                 }
             }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    /// <summary>
    ///     Sets the value of a field and raises property-changing and changed notifications.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected sealed class PropertyChangeTracker {
        private readonly Dictionary<string, IWatch> _map = new(StringComparer.Ordinal);

        public PropertyChangeTracker Track<T>(string name, Func<T> getter, IEqualityComparer<T>? cmp = null) {
            var w = new Watch<T>(getter, cmp ?? EqualityComparer<T>.Default);
            _map[name] = w;
            w.Prime(); // take initial snapshot
            return this;
        }

        public bool IsTracked(string name) => _map.ContainsKey(name);
        public bool HasChanged(string name) => _map.TryGetValue(name, out var w) && w.HasChanged();

        private interface IWatch {
            bool HasChanged();
            void Prime();
        }

        private sealed class Watch<T> : IWatch {
            private readonly Func<T>              _getter;
            private readonly IEqualityComparer<T> _cmp;
            private          T?                   _last;
            private          bool                 _primed;

            public Watch(Func<T> getter, IEqualityComparer<T> cmp) {
                _getter = getter;
                _cmp = cmp;
            }

            public void Prime() {
                _last = _getter();
                _primed = true;
            }

            public bool HasChanged() {
                var cur = _getter();
                if (!_primed) {
                    _last = cur;
                    _primed = true;
                    return true;
                }
                if (_cmp.Equals(cur!, _last!)) return false;
                _last = cur;
                return true;
            }
        }
    }

    protected static class Tolerance {
        public static IEqualityComparer<double> Double(double epsilon) => new DoubleEq(epsilon);

        private sealed class DoubleEq : IEqualityComparer<double> {
            private readonly double _eps;
            public DoubleEq(double eps) => _eps = eps;
            public bool Equals(double x, double y) => Math.Abs(x - y) <= _eps;
            public int GetHashCode(double obj) => obj.GetHashCode();
        }
    }

    public override string ToString() => $"{Entity?.EntityName},{Entity?.Guid},{Entity?.Parent?.Title}";
}