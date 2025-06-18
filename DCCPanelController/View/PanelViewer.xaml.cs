using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class PanelViewer {
    private readonly ConnectionService? _connectionService;
    private readonly PanelViewerViewModel? _viewModel;
    private int activeSpan = -1;
    private int minSpan = 1;
    private int maxSpan = 5;

    public PanelViewer(PanelViewerViewModel viewModel, ConnectionService connectionService) {
        InitializeComponent();
        _connectionService = connectionService;
        _viewModel = viewModel;
        _viewModel.NavigationService = Navigation;
        BindingContext = viewModel;
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        SetLayoutSpan(width, height);
    }

    private void SetLayoutSpan(double width, double height) {
        if (PanelsLayout == null || PanelsCollectionView == null) return;
        if (_viewModel is null) return;

        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;

        if (DeviceInfo.Platform == DevicePlatform.iOS) {
            if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                if (width < height) {
                    activeSpan = 1;
                    minSpan = 1;
                    maxSpan = 2;
                    PanelsCollectionView.ItemTemplate = (DataTemplate)Resources["HorizontalTemplate"];
                } else {
                    SetWideScreenLayout(width);
                }
            } else {
                SetWideScreenLayout(width);
            }
        } else {
            SetWideScreenLayout(width);
        }
        SetZoomLevel();
    }

    private void SetWideScreenLayout(double width) {

        switch (width) {
        case > 1000:
            activeSpan = (activeSpan == -1) ? 3 : activeSpan;
            maxSpan = 5;
            break;
        case > 800:
            activeSpan = (activeSpan == -1) ? 2 : activeSpan;
            maxSpan = 3;
            break;
        default:
            activeSpan = (activeSpan == -1) ? 1 : activeSpan;
            maxSpan = 2;
            break;
        }

        PanelsLayout.Span = activeSpan;
        PanelsCollectionView.ItemTemplate = (DataTemplate)Resources["VerticalTemplate"];
    }
    private void SetZoomLevel(int? adjustment = null) {
        if (adjustment is { } adjustBy ) activeSpan += adjustBy;
        if (activeSpan < minSpan) activeSpan = minSpan;
        if (activeSpan > maxSpan) activeSpan = maxSpan;
        ZoomOutIcon.IsEnabled = activeSpan < maxSpan;
        ZoomInIcon.IsEnabled = activeSpan > minSpan;
        PanelsLayout.Span = activeSpan;
    }

    private void OnZoomInClicked(object? sender, EventArgs e) => SetZoomLevel(-1);
    private void OnZoomOutClicked(object? sender, EventArgs e) => SetZoomLevel(+1);

}