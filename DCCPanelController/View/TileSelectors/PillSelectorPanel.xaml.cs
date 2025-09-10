using System.Collections;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Controls;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanel : ContentView {
    public event EventHandler<TileSelectorDockSide>? OnDockSideChanged;
    private double pillWidth = 600;
    private double scrollOffset = 0;

    public static readonly BindableProperty PanelProperty =
        BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(PillSelectorPanel), propertyChanged: OnPanelChanged);

    public Panel? Panel {
        get => (Panel?)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public void ForceReDraw() => (BindingContext as PillSelectorPanelViewModel)?.ForceReDraw();

    private PillSelectorPanelViewModel? Vm => BindingContext as PillSelectorPanelViewModel;

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (BindingContext is PillSelectorPanelViewModel vm) {
            vm.SelectedCategory = e?.NewValue?.Text ?? string.Empty;
            OnPropertyChanged(nameof(vm.TilesForSelectedCategory));
        }
    }

    public PillSelectorPanel() {
        InitializeComponent();
        var viewModel = new PillSelectorPanelViewModel();
        BindingContext = viewModel;
        SegmentedControl.SelectedIndex = 1;
    }

    private void SizePill() {
        if (BindingContext is PillSelectorPanelViewModel vm && pillWidth > 0 && vm.Categories.Count > 0) {
            PillSelectorGrid.WidthRequest = pillWidth;
            SegmentedControl.SegmentWidth = pillWidth / vm.Categories.Count;
        }
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) {
        pillWidth = widthConstraint switch {
            < 400 => 320,
            < 600 => 400,
            _     => 500
        };
        SizePill();
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PillSelectorPanel { BindingContext: PillSelectorPanelViewModel vm } selector) {
            if (newValue != oldValue && newValue is Panel panel) {
                vm.Panel = panel ?? throw new NullReferenceException("Panels cannot be null");
                selector.SizePill();
            }
        }
    }

    private void SwitchPillPosition(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Side);
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SetDragPreview(sender, e, "copy.png");
        var pointerRoot = e.GetPosition(TileCollection);
        if (!pointerRoot.HasValue) return;

        var index = CollectionHitIndex.IndexOf(TileCollection,
                                               point: pointerRoot.Value,
                                               scrollXOffset: scrollOffset,
                                               scrollYOffset: scrollOffset,
                                               edgeMargin: 4,
                                               topMargin: 4,
                                               itemWidth: 40,
                                               itemHeight: 40,
                                               spacingH: 4,
                                               spacingV: 4);
        if (index is not null) {
            var tile = Vm?.TilesForSelectedCategory[index.Value];
            if (e.Data.Properties is { } props) {
                props["Tile"] = tile;
                return;
            } 
        }
        Console.WriteLine("Unable to find tile at pointer location: Should not happen.");
        e.Cancel = true;
    }

    private void SetDragPreview(object? sender, DragStartingEventArgs e, string imageName) {
        if (string.IsNullOrEmpty(imageName)) return;
        
        // Temporarily disabled this as it was causing a CLIENT ERROR
        // -----------------------------------------------------------
        // #if IOS || MACCATALYST
        // Func<UIKit.UIDragPreview> action = () => {
        //     Console.WriteLine("Setting drag preview in Pill Selector");
        //     var image = UIKit.UIImage.FromFile(imageName);
        //     UIKit.UIImageView imageView = new UIKit.UIImageView(image);
        //     imageView.ContentMode = UIKit.UIViewContentMode.Center;
        //     imageView.Frame = new CoreGraphics.CGRect(0, 0, 32, 32);
        //     return new UIKit.UIDragPreview(imageView);
        // };
        // e?.PlatformArgs?.SetPreviewProvider(action);
        // #endif
    }

    private void TileCollection_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) {
        scrollOffset = e.HorizontalOffset;
    }
}