using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class PanelViewer {
    private readonly ConnectionService? _connectionService;
    private readonly PanelViewerViewModel? _viewModel;

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
                    PanelsLayout.Span = 1;
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
    }

    private void SetWideScreenLayout(double width) {
        PanelsLayout.Span = width switch {
            > 1000 => 4,
            > 800  => 3,
            > 400  => 2,
            _      => 1
        };
        PanelsCollectionView.ItemTemplate = (DataTemplate)Resources["VerticalTemplate"];
    }
}