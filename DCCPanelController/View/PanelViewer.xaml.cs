using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class PanelViewer {
    private readonly ILogger<PanelViewer> _logger;
    private readonly ConnectionService? _connectionService;
    private readonly PanelViewerViewModel? _viewModel;

    private int _activeSpan = 3;
    private int _minSpan = 1;
    private int _maxSpan = 5;

    public PanelViewer(ILogger<PanelViewer> logger, PanelViewerViewModel viewModel, ConnectionService connectionService) {
        InitializeComponent();
        _logger = logger;
        _connectionService = connectionService;
        _viewModel = viewModel;
        _viewModel.NavigationService = Navigation;
        BindingContext = viewModel;
        
        DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;

        
        // Make sure we refresh on a change in the Panels Collection
        // ---------------------------------------------------------
        _viewModel.PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelViewerViewModel.Panels)) SetActiveZoomIcons();
        };
    }

    private void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) {
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => {
            SetLayoutSpan(Width, Height);
        });
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

        // Detect small screen (phone in portrait)
        var isSmallScreen  = (DeviceInfo.Platform == DevicePlatform.iOS && 
                             DeviceInfo.Current.Idiom == DeviceIdiom.Phone) || 
                             (width < height && width < 400);

        if (isSmallScreen) {
            _activeSpan = 1;
            _minSpan = 1;
            _maxSpan = 1;
            PanelsLayout.Span = 1;
            PanelsCollectionView.ItemTemplate = (DataTemplate)Resources["HorizontalTemplate"];
            _viewModel.CanZoomOut = false;
            _viewModel.CanZoomIn = false;

            // We can hide Toolbar Items, only remove them. Get rid of them on small screens
            // -----------------------------------------------------------------------------
            ToolbarItems.Remove(ZoomOutIcon);
            ToolbarItems.Remove(ZoomInIcon);
        } else {
            SetWideScreenLayout(width);
            SetZoomLevel();
        }
    }
    private void SetWideScreenLayout(double width) {
        switch (width) {
        case > 1000:
            _activeSpan = (_activeSpan == -1) ? 3 : _activeSpan;
            _maxSpan = 5;
            break;
        case > 800:
            _activeSpan = (_activeSpan == -1) ? 2 : _activeSpan;
            _maxSpan = 3;
            break;
        default:
            _activeSpan = (_activeSpan == -1) ? 1 : _activeSpan;
            _maxSpan = 2;
            break;
        }

        PanelsLayout.Span = _activeSpan;
        PanelsCollectionView.ItemTemplate = (DataTemplate)Resources["VerticalTemplate"];
        SetActiveZoomIcons();
    }
    
    private void SetZoomLevel(int? adjustment = null) {
        if (adjustment is { } adjustBy ) _activeSpan += adjustBy;
        if (_activeSpan < _minSpan) _activeSpan = _minSpan;
        if (_activeSpan > _maxSpan) _activeSpan = _maxSpan;
        PanelsLayout.Span = _activeSpan;
        SetActiveZoomIcons();
    }

    private void SetActiveZoomIcons() {
        _viewModel?.CanZoomOut = _viewModel.Panels.Count > 0 && _activeSpan < _maxSpan;
        _viewModel?.CanZoomIn  = _viewModel.Panels.Count > 0 && _activeSpan > _minSpan;
    }

    private void OnZoomInClicked(object? sender, EventArgs e) => SetZoomLevel(-1);
    private void OnZoomOutClicked(object? sender, EventArgs e) => SetZoomLevel(+1);

}