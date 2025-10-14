using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;
#if IOS || MACCATALYST
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanel : ContentView {
    public static readonly BindableProperty PanelProperty =
        BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(PillSelectorPanel), propertyChanged: OnPanelChanged);

    private double _pillWidth = 600;
    private double _scrollOffset;

    public PillSelectorPanel() {
        InitializeComponent();
        BindingContext = new PillSelectorPanelViewModel();
        SegmentedControl.SelectedIndex = -1;
    }

    public Panel? Panel {
        get => (Panel?)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    private PillSelectorPanelViewModel? Vm => BindingContext as PillSelectorPanelViewModel;
    public event EventHandler<PaletteDockSide>? OnDockSideChanged;

    public void ForceReDraw() => (BindingContext as PillSelectorPanelViewModel)?.ForceReDraw();

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (BindingContext is PillSelectorPanelViewModel vm) {
            vm.SelectedCategory = e?.NewValue?.Text ?? string.Empty;
            OnPropertyChanged(nameof(vm.TilesForSelectedCategory));
        }
    }

    private void SizePill() {
        if (BindingContext is PillSelectorPanelViewModel vm && _pillWidth > 0 && vm.Categories.Count > 0) {
            PillSelectorGrid.WidthRequest = _pillWidth;
            SegmentedControl.SegmentWidth = _pillWidth / vm.Categories.Count;

            // Fix to make sure the segment is correctly shown
            var index = SegmentedControl.SelectedIndex;
            SegmentedControl.SelectedIndex = -1;
            SegmentedControl.SelectedIndex = index;
        }
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) {
        _pillWidth = widthConstraint switch {
            < 400 => 320,
            < 600 => 400,
            _     => 500,
        };
        SizePill();
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PillSelectorPanel { BindingContext: PillSelectorPanelViewModel vm } selector) {
            if (newValue != oldValue && newValue is Panel panel) {
                vm.Panel = panel ?? throw new NullReferenceException("Panels cannot be null");
                vm.SelectedCategory = vm.Categories[1];
                selector.SegmentedControl.SelectedIndex = 1;
            }
        }
    }

    private void SwitchPillPosition(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, PaletteDockSide.Side);
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SetDragPreviewHelper.SetDragPreview(sender, e, "copy.png");
        var pointerRoot = e.GetPosition(TileCollection);
        if (!pointerRoot.HasValue) return;

        var index = CollectionHitIndex.IndexOf(TileCollection,
            pointerRoot.Value,
            _scrollOffset,
            _scrollOffset,
            4,
            4,
            40,
            40,
            4,
            4);
        if (index is { }) {
            var tile = Vm?.TilesForSelectedCategory[index.Value];
            if (e.Data.Properties is { } props) {
                props["Tile"] = tile;
                return;
            }
        }
        e.Cancel = true;
    }

    private void TileCollection_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) => _scrollOffset = e.HorizontalOffset;
}