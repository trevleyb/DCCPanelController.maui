using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<OperatePage> _logger;
    private bool _tabBarState = true;

    public OperatePage(ILogger<OperatePage> logger, OperateViewModel viewModel) {
        _logger = logger;
        BindingContext = viewModel;
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        InitializeComponent();
        SetTabBarState(true);
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is OperateViewModel viewModel ) {
            if (viewModel.ActivePanel is not null) {
                PanelView.IsVisible = true;
                PanelView.Panel = viewModel.ActivePanel;
                PanelView.BackgroundColor = viewModel.ActivePanel.PanelBackgroundColor;
                BackgroundColor = viewModel.ActivePanel.DisplayBackgroundColor;
                PanelView.ForceRefresh();
            }
            viewModel.UpdatePanelIndicators();
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(OperateViewModel.ActivePanel)) {
            if (BindingContext is OperateViewModel {ActivePanel: not null} viewModel ) {
                PanelView.Panel = viewModel.ActivePanel;
                PanelView.BackgroundColor = viewModel.ActivePanel.PanelBackgroundColor;
                BackgroundColor = viewModel.ActivePanel.DisplayBackgroundColor;
            }
        }
    }

    private async void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        try {
            if (BindingContext is OperateViewModel viewModel) {
                if (e.Tile is ITileInteractive { } tile) {
                    if (e.IsSingleTap) await tile.Interact(viewModel.ConnectionService);
                    if (e.IsDoubleTap) await tile.Secondary(viewModel.ConnectionService);
                }
            }
        } catch (Exception ex) {
            _logger.LogError("OperatePage: PanelViewTileTapped: Error=>{ExMessage}", ex.Message); 
        }
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new HelpPage());
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