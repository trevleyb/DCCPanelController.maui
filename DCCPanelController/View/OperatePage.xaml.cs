using System.ComponentModel;
using System.Diagnostics;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.ControlPanel;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<OperatePage> _logger;
    private readonly ProfileService?      _profileService;
    private readonly OperateViewModel     _viewModel;
    private          ConnectionService?   _connectionService;

    public OperatePage(ILogger<OperatePage> logger, OperateViewModel viewModel, ProfileService profileService, ConnectionService connectionService) {
        InitializeComponent();

        _logger = logger;
        _profileService = profileService;
        _connectionService = connectionService;
        _connectionService.ConnectionStateChanged += ConnectionServiceOnConnectionStateChanged;
        _viewModel = viewModel;
        _viewModel.CurrentPanelIndex = 0;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        BindingContext = _viewModel;
        SetTabBarState(!(_profileService?.ActiveProfile?.Settings?.StartFullScreen ?? true));
        PanelView.Opacity = _connectionService.ConnectionState == DccClientState.Connected ? 1 : 0.25;
    }

    private void ConnectionServiceOnConnectionStateChanged(object? sender, DccClientState e) {
        PanelView.Opacity = e == DccClientState.Connected ? 1 : 0.25;
    }

    protected override async void OnAppearing() {
        try {
            base.OnAppearing();
            await _viewModel.ReselectActivePanelAsync();
        } catch (Exception ex) {
            Console.WriteLine($"Exception in OnAppearing: {ex}");
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(OperateViewModel.ActivePanel)) {
            if (_viewModel is { ActivePanel: { } } viewModel) {
                Title = _profileService?.ActiveProfile?.Settings.TitleBarDisplay switch {
                    TitleBarTextDisplayEnum.Blank       => string.Empty,
                    TitleBarTextDisplayEnum.PanelName   => viewModel.ActivePanel.Title,
                    TitleBarTextDisplayEnum.ProfileName => _profileService.ActiveProfile.ProfileName,
                    _ => $"{_profileService?.ActiveProfile?.ProfileName ?? "DCC Panel Controller"}" 
                };
                PanelView.BackgroundColor = viewModel.PanelBackgroundColor;
                BackgroundColor = viewModel.DisplayBackgroundColor;
            } else {
                Title = $"{_profileService?.ActiveProfile?.ProfileName ?? "DCC Panel Controller"}";
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
        try {
            await HelpService.Current.InitializeAsync();
            await Navigation.PushAsync(new HelpPage());
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to load the Help system: {ex.Message}");
        }
    }

    private async void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        try {
            await AboutPage.ShowAbout();
        } catch { /* Ignore */
        }
    }

    private async void ButtonCloseInstructions(object? sender, EventArgs e) {
        try {
            if (BindingContext is OperateViewModel { Panels.Count: > 0 } viewModel) {
                viewModel.HaveClosedWelcome = true;
                await viewModel.SelectPanelAsync(0);
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to close the Help Welcome page: {ex.Message}");
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
        InvalidateMeasure();
    }
}