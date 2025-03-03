using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly OperateViewModel _viewModel;
    private bool _tabBarState = true;

    public OperatePage() {
        InitializeComponent();
        _viewModel = MauiProgram.ServiceHelper.GetService<OperateViewModel>();
        BindingContext = _viewModel;
        PanelCarousel.CurrentItemChanged += PanelCarouselOnCurrentItemChanged;
        SetTabBarState(true);
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        OnPropertyChanged(nameof(OperateViewModel.Panels));
        OnPropertyChanged(nameof(OperateViewModel.SelectedPanel));
    }

    private void PanelCarouselOnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e) {
        if (BindingContext is OperateViewModel viewModel) {
            Title = viewModel.SetActivePanel(PanelCarousel.CurrentItem as Panel);
            OnPropertyChanged(nameof(viewModel.SelectedPanel));
        }
    }

    private void PanelView_OnTrackPieceChanged(object? sender, ITrack track) { }

    private void PanelView_OnTrackPieceTapped(object? sender, ITrack e) {
        if (e is ITrackInteractive trackPieceTapped) {
            trackPieceTapped.Clicked();
        }
    }

    private void PanelView_OnTrackPieceDoubleTapped(object? sender, ITrack track) {
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

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private void HideUnHideTabBar(object? sender, EventArgs e) {
        SetTabBarState(!_tabBarState);
    }

    private void SetTabBarState(bool state) {
        if (state) {
            Shell.SetTabBarIsVisible(this, true);
            IndicatorView.IsVisible = true;
            HideUnHide.IconImageSource = "maximize_2.png";
        } else {
            Shell.SetTabBarIsVisible(this, false);
            IndicatorView.IsVisible = false;
            HideUnHide.IconImageSource = "minimize_2.png";
        }

        _tabBarState = state;
    }

    private void ConnectButton_OnClicked(object? sender, EventArgs e) {
        _viewModel.Connect();

        var icon = _viewModel.IsConnected switch {
            true  => "circle_green.png",
            false => "circle_red.png",
            null  => "circle_white.png"
        };

        ConnectToolbarButton.IconImageSource = icon;

        //OnPropertyChanged(nameof(ConnectToolbarButton));
    }

}