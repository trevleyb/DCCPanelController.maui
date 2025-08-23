using System.Collections;
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
    public static readonly BindableProperty DockSideProperty = BindableProperty.Create(nameof(DockSide), typeof(TileSelectorDockSide), typeof(SideSelectorPanel), TileSelectorDockSide.Side, BindingMode.TwoWay);
    public SideSelectorPanelViewModel ViewModel { get; set; }

    private double scrollOffset = 0;

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

    private void SwitchSidePosition(object? _, TouchStatusChangedEventArgs e) {
        if (e.Status == TouchStatus.Completed) OnDockSideChanged?.Invoke(this, TileSelectorDockSide.Bottom);
    }

    private void OnTileCollectionDragStarting(object? sender, DragStartingEventArgs e) {
        SetDragPreview(sender, e, "copy.png");

        var child = (sender as GestureRecognizer)?.Parent;
        if (child is not CollectionView childView) return;

        var pointerRoot = e.GetPosition(TileCollection);
        var pointerChild = e.GetPosition(childView);
        if (!pointerRoot.HasValue || !pointerChild.HasValue) return;

        var index = CollectionHitIndex.IndexOf(childView,
                                               point: pointerChild.Value,
                                               scrollXOffset: scrollOffset,
                                               scrollYOffset: scrollOffset,
                                               edgeMargin: 4,
                                               topMargin: 4,
                                               itemWidth: 40,
                                               itemHeight: 40,
                                               spacingH: 4,
                                               spacingV: 4);

        if (index is not null && childView.BindingContext is string category) {
            if (ViewModel?.ByCategory.TryGetValue(category, out var tiles) == true) {
                if (tiles.Count > index.Value) {
                    var tile = tiles[index.Value];
                    if (e.Data.Properties is { } props) {
                        e.Data.Properties["Tile"] = tile;
                        props["Source"] = "Symbol";
                    }
                }
            }
        }
    }

    private void SetDragPreview(object? sender, DragStartingEventArgs e, string imageName) {
#if IOS || MACCATALYST
        Func<UIKit.UIDragPreview> action = () => {
            var image = UIKit.UIImage.FromFile(imageName);
            UIKit.UIImageView imageView = new UIKit.UIImageView(image);
            imageView.ContentMode = UIKit.UIViewContentMode.Center;
            imageView.Frame = new CoreGraphics.CGRect(0, 0, 32, 32);
            return new UIKit.UIDragPreview(imageView);
        };
        e?.PlatformArgs?.SetPreviewProvider(action);
#endif
    }

    private void TileCollection_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) {
        scrollOffset = e.HorizontalOffset;
    }
}