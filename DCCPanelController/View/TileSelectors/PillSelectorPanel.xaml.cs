using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Toolkit;
using Syncfusion.Maui.Toolkit.Accordion;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanel : ContentView {

    public event EventHandler<TileSelectorDockSide>? OnDockSideChanged;
    private double pillWidth = 600;
    
    public static readonly BindableProperty PanelProperty =
        BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(PillSelectorPanel), propertyChanged: OnPanelChanged);

    public Panel? Panel {
        get => (Panel?)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

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
        if (BindingContext is PillSelectorPanelViewModel vm && pillWidth > 0 && vm.Categories.Count>0) {
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
        if (bindable is PillSelectorPanel { BindingContext: PillSelectorPanelViewModel  vm } selector) {
            if (newValue != oldValue && newValue is Panel panel) {
                Console.WriteLine("Panel Changed: Setting up the tiles. ");
                vm.Panel = panel ?? throw new NullReferenceException("Panels cannot be null");;
                selector.SizePill();
            }
        }
    }

    private void OnTileDragStarting(object? sender, DragStartingEventArgs e) {
        // Sender is the DragGestureRecognizer attached to the ContentView whose BindingContext is a Tile
        if (sender is DragGestureRecognizer { BindingContext: Tile tile }) {
            if (e.Data.Properties is { } props) {
                props["Tile"] = tile;
                props["Source"] = "Symbol";

#if IOS || MACCATALYST
                UIDragPreview PreviewProvider()
                {
                    var image = UIImage.FromFile("copy.png"); // optional
                    var imageView = new UIImageView(image)
                    {
                        ContentMode = UIViewContentMode.Center,
                        Frame = new CGRect(0, 0, 32, 32)
                    };
                    return new UIDragPreview(imageView);
                }
                e?.PlatformArgs?.SetPreviewProvider(PreviewProvider);
#endif
            }
        }
    }

    private void OnCurrentTouchStatusChangedLeft(object? _ , TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Left);
    }

    private void OnCurrentTouchStatusChangedMiddle(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Middle);
    }

    private void OnCurrentTouchStatusChangedRight(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Right);
    }
}