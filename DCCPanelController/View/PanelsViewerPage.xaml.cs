using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsViewerPage : ContentPage, INotifyPropertyChanged {
    private readonly PanelsViewerViewModel _viewModel;

    public PanelsViewerPage() {
        _viewModel = new PanelsViewerViewModel();
        BindingContext = _viewModel;
        InitializeComponent();
        _viewModel.SelectedPanel = (_viewModel.Panels.Count > 0) ? _viewModel.Panels[0] : null;
        SetSidePanelState(true);
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }
   
    private void Panel_OnDragStarting(object? sender, DragStartingEventArgs e) {
        Console.WriteLine("DragGestureRecognizer_OnDragStarting");
        if (sender is BindableObject { BindingContext: Panel panel }) {
            e.Data.Properties.Add("Panel", panel);
            e.Data.Properties.Add("Source", "Panel");
            e.Data.Properties.Add("Index", _viewModel.Panels.IndexOf(panel));
        }
    }

    private void Panel_OnDrop(object? sender, DropEventArgs e) {
        Console.WriteLine("DropGestureRecognizer_OnDrop");
        if (sender is BindableObject { BindingContext: Panel target }) {
            var source = e?.Data?.Properties["Source"] as string ?? null;
            var panel = e?.Data?.Properties["Panel"] as Panel ?? null;
            var index = e?.Data?.Properties["Index"] as int? ?? -1;
            if (source is not "Panel" || panel is null || index < 0) return;

            var newIndex = _viewModel.Panels.IndexOf(target);
            if (newIndex < 0) return;
            if (index < newIndex) newIndex--;

            _viewModel.Panels.RemoveAt(index);
            _viewModel.Panels.Insert(newIndex, panel);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (var panelIndex = 0; panelIndex < _viewModel.Panels.Count; panelIndex++) {
                _viewModel.Panels[panelIndex].SortOrder = panelIndex + 1;
            }
        }
    }

    private void SidePanelExpander_OnClicked(object? sender, EventArgs e) {
        SetSidePanelState(!_viewModel.IsSidePanelOpen);
    }

    private void SetSidePanelState(bool expanded) {
        if (!expanded) {
            _viewModel.SidePanelWidth = 0;
            _viewModel.IsSidePanelOpen = false;
            SidePanelButton.Text = "Open Panel";
            SidePanelButton.IconImageSource = "side_panel_open.png";
        } else {
            _viewModel.SidePanelWidth = 300;
            _viewModel.IsSidePanelOpen = true;
            SidePanelButton.Text = "Close panel";
            SidePanelButton.IconImageSource = "side_panel_close_filled.png";
        }
        var tempPanel = _viewModel.SelectedPanel;
        _viewModel.SelectedPanel = null;
        _viewModel.SelectedPanel = tempPanel;
    }

    private void ControlPanelView_OnTrackPieceTapped(object? sender, ITrackPiece e) {
        if (e is ITrackInteractive trackPieceTapped) {
            trackPieceTapped.Clicked();
        }
    }
}