using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class Tile : ContentView, ITile, IDisposable {
    protected const float DefaultScaleFactor = 1.5f;
    protected const float SymbolScaleFactor = 0.75f;

    protected bool disposed = false;
    protected readonly TileDisplayMode DisplayMode;
    protected bool HaveVisualPropertiesChanged;
    protected bool HaveDimensionsChanged;
    protected HashSet<string> VisualProperties { get; } = [];
    protected HashSet<string> ChangedProperties { get; } = [];
    protected bool UseClickSounds => Entity?.Parent?.Panels?.Profile?.Settings?.UseClickSounds ?? true;

    private DateTime _lastChangeTime = DateTime.Now;
    private const int DebounceDelay = 75;
    private CancellationTokenSource? _debounceRebuildCts;
    private Dictionary<string, object?> _propertyCache = [];

    public event EventHandler<TileChangedEventArgs>? TileChanged;

    protected Tile(Entity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) {
        Entity = entity;
        GridSize = gridSize;
        DisplayMode = displayMode;

        PropertyChanged += OnPropertyChanged;
        entity.PropertyChanged += OnPropertyChanged;

        VisualProperties.Add(nameof(GridSize));
        VisualProperties.Add(nameof(Entity.Col));
        VisualProperties.Add(nameof(Entity.Row));
        VisualProperties.Add(nameof(Entity.Width));
        VisualProperties.Add(nameof(Entity.Height));
        VisualProperties.Add(nameof(Entity.Layer));
        VisualProperties.Add(nameof(Entity.Opacity));
        VisualProperties.Add(nameof(Entity.Rotation));
        SetContent();
    }
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this); // Only if you had a finalizer
    }

    protected void Dispose(bool disposing) {
        if (!disposed && disposing) {
            Cleanup();
            disposed = true;
        }
    }

    protected virtual void Cleanup() {
        Entity.PropertyChanged -= OnPropertyChanged;
        PropertyChanged -= OnPropertyChanged;
        _debounceRebuildCts?.Cancel();
        _debounceRebuildCts = null;
    }

    public double TileWidth => GridSize * Entity.Width;
    public double TileHeight => GridSize * Entity.Height;
    public Entity Entity { get; init; }

    public void ForceRedraw() {
        HaveDimensionsChanged = false;        
        HaveVisualPropertiesChanged = true;
        ChangedProperties.Add("FORCE-REDRAW");
        RebuildIfNecessary();
    }

    protected abstract Microsoft.Maui.Controls.View? CreateTile();
    protected abstract Microsoft.Maui.Controls.View? CreateSymbol();

    private void SetNormalContent() {
        BindingContext = this;
        Content = CreateTile();
        if (Content != null) {
            Content.ClassId = Entity.Guid.ToString();
            Content.SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), BindingMode.OneWay, source: this));
            Content.SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), BindingMode.OneWay, source: this));
            Content.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), BindingMode.OneWay, source: Entity));
            Content.SetBinding(IsVisibleProperty, new Binding(nameof(Entity.IsEnabled), BindingMode.OneWay, source: Entity));
        }
    }

    private string GetChangedProperties() {
        var sb = new StringBuilder();
        if (ChangedProperties.Count == 0) return "None";

        foreach (var property in ChangedProperties) {
            if (sb.Length > 0) sb.Append(",");
            sb.Append(property);
        }
        ChangedProperties.Clear();
        return sb.ToString();
    }

    private void SetSymbolContent() {
        BindingContext = this;
        BindingContext = this;
        Content = CreateSymbol();
        if (Content != null) {
            Content.WidthRequest = TileWidth;
            Content.HeightRequest = TileHeight;
            Content.ZIndex = Entity.Layer;
        }
    }

    private void SetContent() {
        switch (DisplayMode) {
        case TileDisplayMode.Normal:
            SetNormalContent();
            break;

        case TileDisplayMode.Symbol:
            SetSymbolContent();
            break;

        case TileDisplayMode.Design:
            SetNormalContent();
            break;

        default:
            throw new ArgumentOutOfRangeException();
        }
    }

    protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is { } property && VisualProperties.Contains(property)) {
            HaveDimensionsChanged = e.PropertyName is nameof(Entity.Col) or nameof(Entity.Row) or nameof(Entity.Width) or nameof(Entity.Height) or nameof(Entity.Rotation);
            HaveVisualPropertiesChanged = true;
            ChangedProperties.Add(property);
        }
        HaveVisualPropertiesChanged = true;
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
        _debounceRebuildCts = new CancellationTokenSource();
        var token = _debounceRebuildCts.Token;

        Task.Delay(DebounceDelay, token)
            .ContinueWith(async t => {
                 if (t.IsCanceled) return;
                 try {
                     // Ensure we're on the UI thread for UI updates
                     await MainThread.InvokeOnMainThreadAsync(() => {
                         if (HaveVisualPropertiesChanged) {
                             SetContent();
                             HaveVisualPropertiesChanged = false; // Reset flag
                             _lastChangeTime = DateTime.Now;
                         }
                     });
                     if (HaveDimensionsChanged) OnTileChanged(TileChangeType.Dimensions);
                     // OnTileChanged(TileChangeType.Modified);
                 } catch (Exception) {
                     //Console.WriteLine($"Error rebuilding tile: {ex.Message}");
                 }
             }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    /// <summary>
    ///     Sets the value of a field and raises property-changing and changed notifications.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">The field to be updated.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">
    ///     The name of the property that changed. This parameter is optional and automatically supplied
    ///     by the compiler.
    /// </param>
    /// <returns>
    ///     True if the value of the field was changed, otherwise false.
    /// </returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }


    // Protected method to raise the event
    protected virtual void OnTileChanged(TileChangeType changeType = TileChangeType.Modified) {
        TileChanged?.Invoke(this, new TileChangedEventArgs(this, changeType));
    }

    protected virtual void OnTileChanged(string propertyName, object? oldValue, object? newValue) {
        TileChanged?.Invoke(this, new TileChangedEventArgs(this, propertyName, oldValue, newValue));
    }
    
    // @formatter:off
    public bool IsSelected {get; set => SetField(ref field, value); }
    public double GridSize {get; set => SetField(ref field, value); }
    // @formatter:on
}

public enum TileDisplayMode {
    Normal, Symbol, Design
}