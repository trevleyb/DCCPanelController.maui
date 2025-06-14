using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Tiles;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class TileSelector {
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(TileSelector), propertyChanged: OnPanelChanged);

    private double _initialWidth;
    private double _startX;

    public TileSelector() {
        InitializeComponent();
        BindingContext = new TileSelectorViewModel();
    }

    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (TileSelector)bindable;
        if (newValue != oldValue) control.ForceRefresh();
    }

    public void ForceRefresh() {
        if (BindingContext is TileSelectorViewModel { } vm) {
            vm.BuildTileList(Panel);
            vm.Panel= Panel;
        }
    }

    private void OnResizeHandlePanUpdated(object? sender, PanUpdatedEventArgs e) {
        if (BindingContext is not TileSelectorViewModel viewModel) return;
        
        switch (e.StatusType) {
            case GestureStatus.Started:
                ResizeHandle.Color = Colors.Black;
                _initialWidth = viewModel.SelectorWidth;
                _startX = e.TotalX;
                break;
                
            case GestureStatus.Running:
                ResizeHandle.Color = Colors.Black;
                // Calculate new width (dragging left decreases width, dragging right increases)
                double deltaX = e.TotalX - _startX;
                double newWidth = _initialWidth - deltaX; // Subtract because we're dragging from the left edge
                
                // Apply minimum constraint
                newWidth = Math.Max(32, newWidth);
                
                viewModel.SelectorWidth = newWidth;
                break;
                
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                ResizeHandle.Color = Colors.LightGray;
                // Reset tracking variables
                _initialWidth = 0;
                _startX = 0;
                break;
        }
    }

    /// <summary>
    ///     Capture the Symbol for use on the Control Surface
    /// </summary>
    private void SymbolDragStarting(object? sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer { BindingContext: Tile { } tile }) {
            if (e.Data.Properties is { } properties) {
                properties.Add("Tile", tile);
                properties.Add("Source", "Symbol");

#if IOS || MACCATALYST
                UIDragPreview Action() {
                    var image = UIImage.FromFile("copy.png");
                    var imageView = new UIImageView(image);
                    imageView.ContentMode = UIViewContentMode.Center;
                    imageView.Frame = new CGRect(0, 0, 32, 32);
                    return new UIDragPreview(imageView);
                }

                e?.PlatformArgs?.SetPreviewProvider(Action);
#endif
            }
        }
    }
}