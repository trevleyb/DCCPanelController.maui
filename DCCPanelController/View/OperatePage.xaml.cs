using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<OperatePage> _logger;
    private OperateViewModel _viewModel;
    private ConnectionService? _connectionService;
    private ProfileService? _profileService;
    
    public OperatePage(ILogger<OperatePage> logger, OperateViewModel viewModel,  ProfileService profileService, ConnectionService connectionService) {
        InitializeComponent();
        _logger = logger;
        _profileService = profileService;
        _connectionService = connectionService;

        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        BindingContext = _viewModel;
        SetTabBarState(true);
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        _viewModel.UpdatePanelIndicators();
    }
    
    void OnPanelIndicatorChanged(object sender, SelectionChangedEventArgs e) {
        if (e.CurrentSelection.FirstOrDefault() is PanelIndicator pi &&
            BindingContext is OperateViewModel vm) {
            vm.SelectPanel(pi.Index);
        }
    }


    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(OperateViewModel.ActivePanel)) {
            if (_viewModel is {ActivePanel: not null} viewModel ) {
                Title = $"{viewModel.ActivePanel.Title}";
                //PanelView.Panel = viewModel.ActivePanel;
                PanelView.BackgroundColor = viewModel.PanelBackgroundColor;
                BackgroundColor = viewModel.DisplayBackgroundColor;
            } else {
                Title = $"DCC Panel Controller";
            }
        }
    }

    private async void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        try {
            if (_viewModel is { } viewModel) {
                if (e.Tile is ITileInteractive { } tile) {
                    if (e.IsSingleTap) await tile.Interact(viewModel.ConnectionService);
                    if (e.IsDoubleTap) await tile.Secondary(viewModel.ConnectionService);
                }
            }
        } catch (Exception ex) {
            _logger.LogError("OperatePage: PanelViewTileTapped: Error=>{ExMessage}", ex.Message); 
        }
    }

    private async void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        await HelpService.Current.InitializeAsync();
        await Navigation.PushAsync(new HelpPage());
    }

    private async void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        await AboutPage.ShowAbout();
    }

    private async void ButtonCloseInstructions(object? sender, EventArgs e) {
        if (BindingContext is OperateViewModel { Panels.Count: > 0 } viewModel) {
            viewModel.SelectPanel(0);
        }
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
        if (_viewModel is { } viewModel) viewModel.IsMaximized = state;
    }
}