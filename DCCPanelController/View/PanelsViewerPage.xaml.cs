using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.View;

public partial class PanelsViewerPage {
    private readonly double _maxSidePanelWidth = 500; // Max width for the left column
    private readonly double _minSidePanelWidth = 100; // Min width for the left column
    private readonly PanelsViewerViewModel _viewModel;

    public PanelsViewerPage() {
        _viewModel = MauiProgram.ServiceHelper.GetService<PanelsViewerViewModel>();
        BindingContext = _viewModel;
        InitializeComponent();
        _viewModel.SelectedPanel = _viewModel.Panels.Count > 0 ? _viewModel.Panels[0] : null;
        SetSidePanelState(true);
        SizeChanged += PanelsViewerPage_SizeChanged;
    }

    private void PanelsViewerPage_SizeChanged(object? sender, EventArgs e) {
        // Determine whether to use "Thin" or "Wide" mode
        var width = Width;
        var height = Height;
        var isPortraitMode = width < height; // Portrait
        var isThinWidth = width < 600;       // Thin screen threshold

        _viewModel.IsThinMode = isThinWidth; // || isPortraitMode;

        // Apply "Thin" mode for iPhone Portrait Mode or Thin MacCatalyst Windows
        if (DeviceInfo.Platform == DevicePlatform.iOS && _viewModel.IsThinMode) {
            LeftPanelColumn.Width = new GridLength(1, GridUnitType.Star);
            RightPanelColumn.Width = new GridLength(0);

            //SidePanelButton.IsEnabled = false;
        } else {
            LeftPanelColumn.Width = new GridLength(_viewModel.SidePanelWidth);
            RightPanelColumn.Width = new GridLength(1, GridUnitType.Star);

            //SidePanelButton.IsEnabled = true;
        }
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private void OnSeparatorDrag(object sender, PanUpdatedEventArgs e) {
        if (e.StatusType == GestureStatus.Running) {
            var newWidth = (int)Math.Max(_minSidePanelWidth, Math.Min(_maxSidePanelWidth, _viewModel.SidePanelWidth + e.TotalX));
            ((PanelsViewerViewModel)BindingContext).SidePanelWidth = newWidth;
        }
    }

    private void SidePanelExpander_OnClicked(object? sender, EventArgs e) {
        SetSidePanelState(!_viewModel.IsSidePanelOpen);
    }

    private void SetSidePanelState(bool expanded) {
        if (!expanded) {
            _viewModel.SidePanelWidth = 0;
            _viewModel.IsSidePanelOpen = false;
            //SidePanelButton.Text = "Open Panel";
            //SidePanelButton.IconImageSource = "side_panel_open.png";
        } else {
            _viewModel.SidePanelWidth = 300;
            _viewModel.IsSidePanelOpen = true;
            //SidePanelButton.Text = "Close panel";
            //SidePanelButton.IconImageSource = "side_panel_close_filled.png";
        }

        var tempPanel = _viewModel.SelectedPanel;
        _viewModel.SelectedPanel = null;
        _viewModel.SelectedPanel = tempPanel;
    }

    private void ControlPanelView_OnTrackPieceTapped(object? sender, ITrack e) {
        if (e is ITrackInteractive trackPieceTapped) {
            trackPieceTapped.Clicked();
        }
    }
}