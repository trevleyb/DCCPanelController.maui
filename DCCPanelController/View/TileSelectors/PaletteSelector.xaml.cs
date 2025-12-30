using CommunityToolkit.Maui.Core;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.TileSelectors;

public partial class PaletteSelector {
    public event EventHandler? OnClosePalette;
    public ITile? SelectedTile => SelectedItem?.Tile ?? null;

    public PaletteSelector() {
        InitializeComponent();
        AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    }

    public void SwitchPaletteView() {
        var palette = Palette;
        Palette = null;
        Palette = palette;
    }
    
    private void ClosePalette(object? sender, TouchGestureCompletedEventArgs e) {
        OnClosePalette?.Invoke(this, EventArgs.Empty);
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SelectedItem = null;
        try {
            if ((sender as ContentView)?.BindingContext is PaletteItem item && e.Data.Properties is { } props) {
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

        var newSelection = e.CurrentSelection is { Count: > 0 } ? e.CurrentSelection[0] as PaletteItem : null;
        if (newSelection == SelectedItem) SelectedItem = null;
        if (newSelection != SelectedItem) SelectedItem = newSelection;
    }

    private void OnItemTapped(object? sender, TappedEventArgs e) {
        if (sender is ContentView { BindingContext: PaletteItem item }) {
            SelectedItem = SelectedItem == item ? null : item;
        }
    }

    private void OnGroupTapped(object? sender, TappedEventArgs e) {
        if (sender is VerticalStackLayout { BindingContext: PaletteGroup group }) {
            // Temporarily disabling this as it is not quite right for some reason. 
            // It works, but if I rotate and rotate back the margins don't seem right
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
        }
    }

    private void InstanceOnSelectedTileCleared() => SelectedItem = null;
    
    public static readonly BindableProperty PaletteProperty = BindableProperty.Create(nameof(Palette), typeof(Palette), typeof(PaletteSelector), defaultValue: PaletteCache.GetDefaultPalette());
    public Palette? Palette {
        get => (Palette)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(ItemsLayoutOrientation?), typeof(PaletteSelector), ItemsLayoutOrientation.Vertical, propertyChanged: OnOrientationChanged);
    public ItemsLayoutOrientation Orientation {
        get => (ItemsLayoutOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value); 
    }

    public ItemsLayout ItemLayout =>
        Orientation == ItemsLayoutOrientation.Vertical
            ? (ItemsLayout)new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) {
                VerticalItemSpacing = 2,
                HorizontalItemSpacing = 2,
            }
            : (ItemsLayout)new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal) {
                VerticalItemSpacing = 2,
                HorizontalItemSpacing = 2,
            };
    
    public int CategoryRotation {
        get;
        set {
            field = value;
            OnPropertyChanged(nameof(CategoryRotation));
        }
    }

    // Utility
    public void ScrollToStartIfAny() {
        if (PaletteCollectionView.ItemsSource is System.Collections.ICollection { Count: > 0 })
            PaletteCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PaletteSelector selector) {
            if (newValue is ItemsLayoutOrientation layout) {
                if (layout == ItemsLayoutOrientation.Vertical) {
                    selector.CategoryRotation = 0;
                    selector.SideCloseButton.IsVisible = true;
                    selector.BottomCloseButton.IsVisible = false;
                } else {
                    selector.CategoryRotation = 270;
                    selector.SideCloseButton.IsVisible = false;
                    selector.BottomCloseButton.IsVisible = true;
                }
            }
            selector.OnPropertyChanged(nameof(ItemLayout));
        }
    }

}
