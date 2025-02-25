using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {

    private bool _tabBarState = true;

    public OperatePage() {
        InitializeComponent();
        BindingContext = new OperateViewModel();
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
        Console.WriteLine($"In Operate Mode: Track {e.Name} was tapped");
        if (e is ITrackInteractive trackPieceTapped) {
            trackPieceTapped.Clicked();
        }
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
            HideUnHide.IconImageSource = "maximize_2.png";
        } else {
            Shell.SetTabBarIsVisible(this, false);
            HideUnHide.IconImageSource = "minimize_2.png";
        }

        _tabBarState = state;
    }
}