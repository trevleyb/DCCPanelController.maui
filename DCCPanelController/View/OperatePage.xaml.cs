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
using Fonts;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<OperatePage> _logger;
    private OperateViewModel? _viewModel;
    private ConnectionService? _connectionService;
    private ProfileService? _profileService;
    
    public OperatePage(ILogger<OperatePage> logger, ProfileService profileService, ConnectionService connectionService) {
        InitializeComponent();
        _logger = logger;
        _profileService = profileService;
        _connectionService = connectionService;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        Title = "DCC Panel Controller";

        var currentPanelIndex = -1;
        var showWelcomePage = true;
        
        if (_viewModel is not null) {
            currentPanelIndex = _viewModel.CurrentPanelIndex;
            showWelcomePage = _viewModel.ShowWelcomePage;
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModel = null;
            BindingContext = null;
        }

        _viewModel = MauiProgram.ServiceHelper.GetService<OperateViewModel>();
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.ShowWelcomePage = showWelcomePage;
        _viewModel.CurrentPanelIndex = currentPanelIndex;
        BindingContext = _viewModel;

        // We reload the Panels at this point, whenever this screen is appearing
        // because data may have changed and is out of sync with the ViewModel
        // --------------------------------------------------------------------------
        await _viewModel.LoadPanelsAsync();
        PanelView.Panel = null;
        if (_viewModel.CurrentPanelIndex >= 0) _viewModel.SelectPanel(_viewModel.CurrentPanelIndex);
        SetTabBarState(true);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(OperateViewModel.ActivePanel)) {
            if (_viewModel is OperateViewModel {ActivePanel: not null} viewModel ) {
                Title = $"{viewModel.ActivePanel.Title}";
                PanelView.Panel = viewModel.ActivePanel;
                PanelView.BackgroundColor = viewModel.ActivePanel?.PanelBackgroundColor;
                BackgroundColor = viewModel.ActivePanel?.DisplayBackgroundColor;
            } else {
                PanelView.Panel = null;
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
        if (BindingContext is OperateViewModel { Panels.Count: > 0 } viewModel) viewModel.SelectPanel(0);
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