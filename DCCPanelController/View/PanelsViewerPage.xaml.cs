using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.View;

public partial class PanelsViewerPage {
    private readonly PanelsViewerViewModel _viewModel;

    public PanelsViewerPage() {
        _viewModel = MauiProgram.ServiceHelper.GetService<PanelsViewerViewModel>();
        BindingContext = _viewModel;
        InitializeComponent();
        _viewModel.SelectedPanel = _viewModel.Panels.Count > 0 ? _viewModel.Panels[0] : null;
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private void ControlPanelView_OnTrackPieceTapped(object? sender, ITrack e) {
        if (e is ITrackInteractive trackPieceTapped) trackPieceTapped.Clicked();
    }

    private void ControlPanelView_OnTrackPieceDoubleTapped(object? sender, ITrack track) {
        if (track.Parent?.Tracks != null) {
            var tracks = track.Parent.Tracks;
            if (track.IsPath) {
                TrackPointsValidator.ClearTrackPaths(tracks);
            } else {
                TrackPointsValidator.MarkTrackPaths(tracks,track);
            }
        }
        //var tempPanel = _viewModel.SelectedPanel;
        //_viewModel.SelectedPanel = null;
        //_viewModel.SelectedPanel = tempPanel;
    }
}