using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class Tile : ContentView, ITile {
    protected const float DefaultScaleFactor = 1.5f;
    protected const float SymbolScaleFactor = 0.75f;

    protected readonly TileDisplayMode DisplayMode;
    protected bool HaveVisualPropertiesChanged;
    protected HashSet<string> VisualProperties { get; } = [];
    protected bool UseClickSounds => Entity?.Parent?.Panels?.Profile?.Settings?.UseClickSounds ?? true;

    private const int DebounceDelay = 50;
    private CancellationTokenSource? _debounceRebuildCts;
    private Dictionary<string, object?> _propertyCache = [];
    
    protected Tile(Entity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) {
        Entity = entity;
        GridSize = gridSize;
        DisplayMode = displayMode;

        PropertyChanged += OnPropertyChanged;
        entity.PropertyChanged += OnPropertyChanged;

        VisualProperties.Add(nameof(GridSize));
        VisualProperties.Add(nameof(Entity.Col));
        VisualProperties.Add(nameof(Entity.Row));
        VisualProperties.Add(nameof(Entity.Layer));
        VisualProperties.Add(nameof(Entity.Opacity));
        VisualProperties.Add(nameof(Entity.Rotation));
        SetContent();
    }

    public double TileWidth => GridSize * Entity.Width;
    public double TileHeight => GridSize * Entity.Height;
    public Entity Entity { get; init; }

    public void ForceRedraw() {
        HaveVisualPropertiesChanged = true;
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
        if (DisplayMode == TileDisplayMode.Normal) SetNormalContent();
        if (DisplayMode == TileDisplayMode.Symbol) SetSymbolContent();
        if (DisplayMode == TileDisplayMode.Design) SetNormalContent();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is { } property && VisualProperties.Contains(property)) {
            HaveVisualPropertiesChanged = true;
        }
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
                         } 
                     });
                 } catch (Exception ex) {
                     Console.WriteLine($"Error rebuilding tile: {ex.Message}");
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

    // @formatter:off
    public bool IsSelected {get; set => SetField(ref field, value); }
    public double GridSize {get; set => SetField(ref field, value); }
    // @formatter:on
}

public enum TileDisplayMode {
    Normal, Symbol, Design
}