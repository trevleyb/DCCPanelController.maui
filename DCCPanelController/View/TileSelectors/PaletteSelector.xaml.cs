using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.TileSelectors;

public partial class PaletteSelector {

    public event EventHandler<PaletteDockSide>? OnDockSideChanged;
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(PaletteDockSide), typeof(PaletteSelector), PaletteDockSide.Side, BindingMode.TwoWay);

    public PaletteSelector() {
        InitializeComponent();
        ViewModel = new PaletteSelectorViewModel();
        BindingContext = ViewModel;
    }

    public PaletteSelectorViewModel ViewModel { get; init; }
    public PaletteDockSide DockSide {
        get => (PaletteDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    private void SwitchDockPosition(object? _, object e) {
        ViewModel.DockSide = ViewModel.DockSide == PaletteDockSide.Bottom ? PaletteDockSide.Side : PaletteDockSide.Bottom;
        switch (ViewModel.DockSide) {
        case PaletteDockSide.Side:
            //TileCollectionByCategoryLandscape.IsVisible = true;
            //TileCollectionByCategoryPortrait.IsVisible = false;
            break;
        case PaletteDockSide.Bottom:
            //TileCollectionByCategoryLandscape.IsVisible = false;
            //TileCollectionByCategoryPortrait.IsVisible = true;
            break;
        }
        OnDockSideChanged?.Invoke(this, ViewModel.DockSide);
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        ViewModel.SelectedItem = null;
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
        ViewModel.SelectedItem = null;
        if (e.CurrentSelection is { Count: > 0 }) ViewModel.SelectedItem = e.CurrentSelection[0] as PaletteItem;
    }

    private void OnItemTapped(object? sender, TappedEventArgs e) {
        if (sender is ContentView { BindingContext: PaletteItem item }) {
            ViewModel.SelectedItem = item;
        }
    }

    private void OnGroupTapped(object? sender, TappedEventArgs e) {
        if (sender is VerticalStackLayout { BindingContext: PaletteGroup group }) {
            group.ToggleExpandCommand.Execute(null);            
        }
    }
}