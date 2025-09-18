using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
#if IOS || MACCATALYST
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class SideSelectorPanel {
    public static readonly BindableProperty PanelProperty    = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(SideSelectorPanel), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(TileSelectorDockSide), typeof(SideSelectorPanel), TileSelectorDockSide.Side, BindingMode.TwoWay);

    public static readonly BindableProperty SelectedTileProperty =
        BindableProperty.Create(
            nameof(SelectedTile),
            typeof(ITile),
            typeof(SideSelectorPanel),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedTileChanged);

    private double scrollOffset;

    public SideSelectorPanel() {
        InitializeComponent();
        ViewModel = new SideSelectorPanelViewModel();
        BindingContext = ViewModel;
    }

    public SideSelectorPanelViewModel ViewModel { get; set; }

    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public TileSelectorDockSide DockSide {
        get => (TileSelectorDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    public ITile? SelectedTile {
        get => (ITile?)GetValue(SelectedTileProperty);
        set => SetValue(SelectedTileProperty, value);
    }

    public event EventHandler<TileSelectorDockSide>? OnDockSideChanged;

    public void ForceReDraw() => (BindingContext as SideSelectorPanelViewModel)?.ForceReDraw();

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is SideSelectorPanel { BindingContext: SideSelectorPanelViewModel vm }) {
            if (newValue != oldValue && newValue is Panel panel) {
                vm.Panel = panel ?? throw new NullReferenceException("Panels cannot be null");
            }
        }
    }

    private void SwitchSidePosition(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Bottom);
    }

    private static void OnSelectedTileChanged(BindableObject bindable, object oldValue, object newValue) {
        var view = (SideSelectorPanel)bindable;

        // keep VM in sync if you’re using one
        if (view.BindingContext is SideSelectorPanelViewModel vm) {
            if (!ReferenceEquals(vm.SelectedTile, newValue)) {
                vm.SelectedTile = (ITile?)newValue;
            }
        }

        // (optional) if your VM can also change SelectedTile, you might already
        // mirror that back to the control in OnBindingContextChanged or via VM event handlers.
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is SideSelectorPanelViewModel vm) {
            // keep control property synced from VM, too
            if (!ReferenceEquals(SelectedTile, vm.SelectedTile)) {
                SelectedTile = vm.SelectedTile;
            }

            // if you have an event in VM when SelectedTile changes, subscribe and mirror here
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
                scrollOffset,
                scrollOffset,
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
            Debug.WriteLine("Error selecting tile: " + ex.Message);
        }
    }

    private void TileCollection_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) => scrollOffset = e.HorizontalOffset;

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