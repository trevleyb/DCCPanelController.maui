using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Helpers;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace DCCPanelController.View.TileSelectors;

public partial class SideSelectorPanel {
    public event EventHandler<TileSelectorDockSide>? OnDockSideChanged;
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(SideSelectorPanel), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(TileSelectorDockSide), typeof(SideSelectorPanel), TileSelectorDockSide.Middle, BindingMode.TwoWay, propertyChanged: OnDockSidePanelChanged);
    public SideSelectorPanelViewModel ViewModel { get; set; }

    public SideSelectorPanel() {
        InitializeComponent();
        ViewModel = new SideSelectorPanelViewModel();
        BindingContext = ViewModel;
    }
    
    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public void ForceReDraw() => (BindingContext as SideSelectorPanelViewModel)?.ForceReDraw();

    public TileSelectorDockSide DockSide {
        get => (TileSelectorDockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is SideSelectorPanel { BindingContext: SideSelectorPanelViewModel vm }) {
            if (newValue != oldValue && newValue is Panel panel) {
                vm.Panel = panel ?? throw new NullReferenceException("Panels cannot be null");
            }
        }
    }
    
    private static void OnDockSidePanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is SideSelectorPanel selector && newValue is TileSelectorDockSide side) {
            switch (side) {
            case TileSelectorDockSide.Left:
                selector.LeftDockButton.IsEnabled = false;
                selector.RightDockButton.IsEnabled = true;
                selector.LeftDockButton.Stroke = StyleHelper.FromStyle("PrimaryDisabled");
                selector.RightDockButton.Stroke = StyleHelper.FromStyle("Primary");
                break;
            case TileSelectorDockSide.Middle:
                break;
            case TileSelectorDockSide.Right:
                selector.LeftDockButton.IsEnabled = true;
                selector.RightDockButton.IsEnabled = false;
                selector.LeftDockButton.Stroke = StyleHelper.FromStyle("Primary");
                selector.RightDockButton.Stroke = StyleHelper.FromStyle("PrimaryDisabled");
                break;
            }
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
                e.PlatformArgs?.SetPreviewProvider(Action);
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