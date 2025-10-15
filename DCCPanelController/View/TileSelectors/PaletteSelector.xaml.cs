using CommunityToolkit.Maui.Core;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.TileSelectors;

public partial class PaletteSelector {
    public event EventHandler<PaletteDockSide>? OnDockSideChanged;
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(PaletteDockSide), typeof(PaletteSelector), PaletteDockSide.Side, BindingMode.TwoWay, propertyChanged: DockSidePropertyChanged);

    public Palette Palette { get; set; } = PaletteCache.GetDefaultPalette();
    public ITile? SelectedTile => SelectedItem?.Tile ?? null;

    public PaletteSelector() {
        InitializeComponent();
        AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    }

    public PaletteDockSide DockSide {
        get => (PaletteDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    private static void DockSidePropertyChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (PaletteSelector)bindable;
        var side = (PaletteDockSide)newValue;

        control.ApplyDockSide(side);                      // <- update internal layout
        control.OnDockSideChanged?.Invoke(control, side); // <- tell container
    }

    private void SwitchDockPosition(object? sender, TouchGestureCompletedEventArgs e) {
        DockSide = DockSide == PaletteDockSide.Side ? PaletteDockSide.Bottom : PaletteDockSide.Side;

        //ApplyDockSide(DockSide);
    }

    private void ApplyDockSide(PaletteDockSide side) {
        // 1) Build the target layout (fresh instance)
        var grid = side == PaletteDockSide.Side
            ? new GridItemsLayout(ItemsLayoutOrientation.Vertical) { Span = 2, VerticalItemSpacing = 2, HorizontalItemSpacing = 2 }
            : new GridItemsLayout(ItemsLayoutOrientation.Horizontal) { Span = 2, VerticalItemSpacing = 2, HorizontalItemSpacing = 2 };

        // 2) Nudge the handler cache: flip to a temporary Linear layout first
        var temp = side == PaletteDockSide.Side
            ? new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            : new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

        // IMPORTANT: do all of this on the UI thread
        MainThread.BeginInvokeOnMainThread(async () => {
            
            // 0) Detach items briefly (breaks virtualization state cleanly)
            var src = PaletteCollectionView.ItemsSource;
            PaletteCollectionView.ItemsSource = null;
            
            // Swap to temp to invalidate the native layout pipeline
            PaletteCollectionView.ItemsLayout = temp;

            // Ensure consistent cell measurement (your tiles are uniform)
            PaletteCollectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;

            // Yield one tick so the native layout tears down
            await Task.Yield();

            // Now apply the real Grid layout
            PaletteCollectionView.ItemsLayout = grid;

            // Reset scroll (prevents “drift” illusion when rows/columns change)
            PaletteCollectionView.ItemsSource = src;
            if (src is System.Collections.ICollection { Count: > 0 })
                PaletteCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
            
            // Re-measure this control + parent
            PaletteCollectionView.InvalidateMeasure();
            InvalidateMeasure();
            (Parent as VisualElement)?.InvalidateMeasure();
        });
    }

    private void ApplyDockSidex(PaletteDockSide side) {
        var itemsLayout =
            side == PaletteDockSide.Side
                ? new GridItemsLayout(ItemsLayoutOrientation.Vertical) { Span = 2, VerticalItemSpacing = 2, HorizontalItemSpacing = 2 }
                : new GridItemsLayout(ItemsLayoutOrientation.Horizontal) { Span = 2, VerticalItemSpacing = 2, HorizontalItemSpacing = 2 };

        PaletteCollectionView.ItemsLayout = null;
        PaletteCollectionView.ItemsLayout = itemsLayout;

        // Force a fresh measure/layout pass on this control and its parent
        MainThread.BeginInvokeOnMainThread(() => {
            PaletteCollectionView.InvalidateMeasure();
            InvalidateMeasure();
            (Parent as VisualElement)?.InvalidateMeasure();
        });
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SelectedItem = null;
        try {
            if ((sender as GestureRecognizer)?.Parent?.BindingContext is PaletteItem item && e.Data.Properties is { } props) {
                props["Tile"] = item.Tile;
                return;
            }

            e.Cancel = true;
        } catch (Exception ex) {
            LogHelper.Logger.LogWarning("Error selecting tile: " + ex.Message);
        }
    }

    private void TileCollection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        // Clear any previously selected Items (should only ever be 1 anyway)
        // -------------------------------------------------------------------
        if (e.PreviousSelection is { Count: > 0 }) {
            foreach (var item in e.PreviousSelection) {
                if (item is PaletteItem paletteItem) paletteItem.IsSelected = false;
            }
        }

        SelectedItem = null;
        if (e.CurrentSelection is { Count: > 0 }) SelectedItem = e.CurrentSelection[0] as PaletteItem;
    }

    private void OnItemTapped(object? sender, TappedEventArgs e) {
        if (sender is ContentView { BindingContext: PaletteItem item }) {
            SelectedItem = item;
        }
    }

    private void OnGroupTapped(object? sender, TappedEventArgs e) {
        if (sender is VerticalStackLayout { BindingContext: PaletteGroup group }) {
            group.ToggleExpandCommand.Execute(null);
        }
    }

    private PaletteItem? _selectedItem;

    public PaletteItem? SelectedItem {
        get => _selectedItem;
        set {
            if (_selectedItem == value) return;
            _selectedItem?.IsSelected = false;
            _selectedItem = value;
            if (_selectedItem == null) return;
            _selectedItem?.IsSelected = true;
            AppStateService.Instance.SelectedTile = value?.Tile ?? null;
            OnPropertyChanged();
            Console.WriteLine($"Selected Tile->{SelectedItem?.Tile.Entity.EntityName}");
        }
    }

    private void InstanceOnSelectedTileCleared() => SelectedItem = null;
}