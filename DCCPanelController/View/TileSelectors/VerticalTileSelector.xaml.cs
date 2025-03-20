using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Tiles;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class VerticalTileSelector {

    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(VerticalTileSelector), propertyChanged: OnPanelChanged);
    
    public VerticalTileSelector() {
        InitializeComponent();
        BindingContext = new VerticalTileSelectorViewModel();
    }
    
    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (VerticalTileSelector)bindable;
        if (newValue != oldValue) control.ForceRefresh();
    }
    
    public void ForceRefresh() {
        if (BindingContext is VerticalTileSelectorViewModel { } vm) {
            vm.BuildTileList(Panel);
        }
    }
    
    /// <summary>
    /// Capture the Symbol for use on the Control Surface
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