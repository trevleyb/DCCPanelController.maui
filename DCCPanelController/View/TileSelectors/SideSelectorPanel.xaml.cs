using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
#if IOS || MACCATALYST
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class SideSelectorPanel {
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(PaletteDockSide), typeof(SideSelectorPanel), PaletteDockSide.Side, BindingMode.TwoWay);

    public static readonly BindableProperty SelectedTileProperty =
        BindableProperty.Create(
            nameof(SelectedTile),
            typeof(ITile),
            typeof(SideSelectorPanel),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedTileChanged);

    private double _scrollOffset;

    public SideSelectorPanel() {
        InitializeComponent();
        ViewModel = new SideSelectorPanelViewModel();
        BindingContext = ViewModel;
    }

    public SideSelectorPanelViewModel ViewModel { get; set; }

    public PaletteDockSide DockSide {
        get => (PaletteDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    public ITile? SelectedTile {
        get => (ITile?)GetValue(SelectedTileProperty);
        set => SetValue(SelectedTileProperty, value);
    }

    public event EventHandler<PaletteDockSide>? OnDockSideChanged;

    private void SwitchSidePosition(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, PaletteDockSide.Bottom);
    }

    private static void OnSelectedTileChanged(BindableObject bindable, object oldValue, object newValue) {
        var view = (SideSelectorPanel)bindable;
        if (view.BindingContext is SideSelectorPanelViewModel vm) {
            if (!ReferenceEquals(vm.SelectedTile, newValue)) {
                vm.SelectedTile = (ITile?)newValue;
            }
        }
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is SideSelectorPanelViewModel vm) {
            if (!ReferenceEquals(SelectedTile, vm.SelectedTile)) {
                SelectedTile = vm.SelectedTile;
            }
            vm.PropertyChanged -= VmOnPropertyChanged;
            vm.PropertyChanged += VmOnPropertyChanged;
        }
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(SideSelectorPanelViewModel.SelectedTile) &&
            sender is SideSelectorPanelViewModel vm &&
            !ReferenceEquals(SelectedTile, vm.SelectedTile)) {
            SelectedTile = vm.SelectedTile;
        }
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SetDragPreviewHelper.SetDragPreview(sender, e, "copy.png");
        SelectedTile = null;

        try {
            var child = (sender as GestureRecognizer)?.Parent;
            if (child is not CollectionView childView) return;

            var pointerRoot = e.GetPosition(TileCollection);
            var pointerChild = e.GetPosition(childView);
            if (!pointerRoot.HasValue || !pointerChild.HasValue) return;

            var index = CollectionHitIndex.IndexOf(childView,
                pointerChild.Value,
                _scrollOffset,
                _scrollOffset,
                4,
                4,
                40,
                40,
                4,
                4);

            if (index is { } && childView.BindingContext is string category) {
                if (ViewModel?.ByCategory.TryGetValue(category, out var tiles) == true) {
                    if (tiles.Count > index.Value) {
                        var tile = tiles[index.Value];
                        if (e.Data.Properties is { } props) {
                            e.Data.Properties["Tile"] = tile;
                            return;
                        }
                    }
                }
            }
            e.Cancel = true;
        } catch (Exception ex) {
            LogHelper.Logger.LogWarning("Error selecting tile: " + ex.Message);
        }
    }

    private void TileCollection_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) => _scrollOffset = e.HorizontalOffset;

    private void OnTileTapped(object? sender, EventArgs e) {
        if (e is not TappedEventArgs te) return;
        if (te.Parameter is not ITile tile) return;

        // Toggle behaviour: tap same tile -> null
        ViewModel.SelectedTile = ReferenceEquals(ViewModel.SelectedTile, tile) ? null : tile;

        // Keep AppState in sync if you still need it
        AppStateService.Instance.SelectedTile = ViewModel.SelectedTile;
    }

    private void TileCollectionByCategory_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        var currentSelection = e.CurrentSelection.ToList().FirstOrDefault() as ITile;
        var previousSelection = e.PreviousSelection.ToList().FirstOrDefault() as ITile;
        if (currentSelection == previousSelection) {
            ViewModel.SelectedTile = null;
        }
        ViewModel.SelectedTile = currentSelection;
        AppStateService.Instance.SelectedTile = ViewModel?.SelectedTile;
    }
}