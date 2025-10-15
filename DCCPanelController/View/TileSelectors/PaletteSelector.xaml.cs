using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.TileSelectors;

public partial class PaletteSelector {

    public event EventHandler<PaletteDockSide>? OnDockSideChanged;
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(PaletteDockSide), typeof(PaletteSelector), PaletteDockSide.Side, BindingMode.TwoWay);

    public Palette Palette { get; set; } = PaletteCache.Palette;
    public ITile? SelectedTile => SelectedItem?.Tile ?? null;

    public PaletteSelector() {
        InitializeComponent();
        BindingContext = this;
        AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    }

    public PaletteDockSide DockSide {
        get => (PaletteDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    private void SwitchDockPosition(object? _, object e) {
        DockSide = DockSide == PaletteDockSide.Bottom ? PaletteDockSide.Side : PaletteDockSide.Bottom;
        switch (DockSide) {
        case PaletteDockSide.Side:
            //WidthRequest = 110;
            //HeightRequest = -1;
            break;
        case PaletteDockSide.Bottom:
            //WidthRequest = -1;
            //HeightRequest = 120;
            break;
        }
        OnDockSideChanged?.Invoke(this, DockSide);
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