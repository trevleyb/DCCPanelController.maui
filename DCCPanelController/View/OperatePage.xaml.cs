using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly OperateViewModel _viewModel;
    private bool _tabBarState = true;

    public OperatePage(OperateViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel; //MauiProgram.ServiceHelper.GetService<OperateViewModel>(profile, connectionService);
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        PanelCarousel.CurrentItemChanged += PanelCarouselOnCurrentItemChanged;
        SetTabBarState(true);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e?.PropertyName) {
        case nameof(OperateViewModel.IsConnected):
            ConnectButton.IconImageSource = _viewModel.IsConnected ? "wifi.png" : "wifi_off.png";
            break;
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        PanelCarousel.ItemsSource = null;
        PanelCarousel.ItemsSource = _viewModel.Panels;
        PanelCarousel.CurrentItem = _viewModel.SelectedPanel;
    }

    private void PanelCarouselOnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e) {
        if (BindingContext is OperateViewModel viewModel) {
            Title = viewModel.SetActivePanel(PanelCarousel.CurrentItem as Panel);
            OnPropertyChanged(nameof(viewModel.SelectedPanel));
        }
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        //Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        //Navigation.PushAsync(new InstructionsPage());
    }

    private void HideUnHideTabBar(object? sender, EventArgs e) {
        SetTabBarState(!_tabBarState);
    }

    private void SetTabBarState(bool state) {
        if (state) {
            Shell.SetTabBarIsVisible(this, true);
            //IndicatorView.IsVisible = true;
            HideUnHide.IconImageSource = "maximize_2.png";
        } else {
            Shell.SetTabBarIsVisible(this, false);
            //IndicatorView.IsVisible = false;
            HideUnHide.IconImageSource = "minimize_2.png";
        }

        _tabBarState = state;
    }
    
    private void ToggleGrid(object? sender, EventArgs e) {
        _viewModel.ShowGrid = !_viewModel.ShowGrid;
    }
}