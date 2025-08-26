using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Fonts;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<OperatePage> _logger;

    public OperatePage(ILogger<OperatePage> logger, OperateViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        BindingContext = viewModel;
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        SetTabBarState(true);
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is OperateViewModel viewModel) {
            await viewModel.LoadPanelsAsync();
            PanelView.Panel = null;
            //viewModel.SelectPanelCommand.Execute(viewModel.CurrentPanelIndex);
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(OperateViewModel.ActivePanel)) {
            if (BindingContext is OperateViewModel {ActivePanel: not null} viewModel ) {
                PanelView.Panel = viewModel.ActivePanel;
                PanelView.BackgroundColor = viewModel.ActivePanel.PanelBackgroundColor;
                BackgroundColor = viewModel.ActivePanel.DisplayBackgroundColor;
            } else {
                PanelView.Panel = null;
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
        if (BindingContext is OperateViewModel viewModel) {
            SetTabBarState(!viewModel.IsMaximized);    
        }
    }

    private void SetTabBarState(bool state) {
        if (state) {
            Shell.SetTabBarIsVisible(this, true);
            HideUnHide.IconImageSource = "maximize_2_active";
        } else {
            Shell.SetTabBarIsVisible(this, false);
            HideUnHide.IconImageSource = "minimize_2_active";
        }
        if (BindingContext is OperateViewModel viewModel) viewModel.IsMaximized = state;
    }
}