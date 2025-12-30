using System.ComponentModel;
using System.Diagnostics;
using DCCPanelController.Clients;
using DCCPanelController.Helpers.Logging;
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
        _logger = logger;
        _profileService = profileService;
        _connectionService = connectionService;
        _connectionService.ConnectionStateChanged += ConnectionServiceOnConnectionStateChanged;
        _viewModel = viewModel;
        _viewModel.CurrentPanelIndex = 0;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        BindingContext = _viewModel;
        InitializeComponent();

        SetChromeVisible(!( _profileService?.ActiveProfile?.Settings?.StartFullScreen ?? false ));
    }

    private void ConnectionServiceOnConnectionStateChanged(object? sender, DccClientState e) { }

    protected override async void OnAppearing() {
        try {
            base.OnAppearing();
            _viewModel.PropertiesChanged(); // Make sure all icons and things are refreshed
            await _viewModel.ReselectActivePanelAsync();
        } catch (Exception ex) {
            _logger.LogError($"Exception in OnAppearing: {ex}");
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
                if (ConnectionService.Instance.ConnectionState != DccClientState.Connected) {
                    await DisplayAlertHelper.DisplayToastAlert("Please connect to the DCC Panel Controller first.", 18D);
                } else {
                    if (e.Tile is ITileInteractive { } tile) {
                        if (e.IsSingleTap) await tile.Interact(viewModel.ConnectionService);
                        if (e.IsDoubleTap) await tile.Secondary(viewModel.ConnectionService);
                    }
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
            _logger.LogError(ex,$"Unable to load the Help system: {ex.Message}");
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
            _logger.LogError(ex, $"Unable to close the Help Welcome page: {ex.Message}");
        }
    }
    
    // Replace your HideUnHideTabBar and SetTabBarState with this:
    private void ToggleChrome(object? sender, EventArgs e) {
        SetChromeVisible(show: _viewModel?.IsMaximized == true);
    }

    private void SetChromeVisible(bool show) {
#if IOS
        Shell.SetTabBarIsVisible(this, show);
#elif MACCATALYST
        // On MacCatalyst, always show the tab bar as it's the primary navigation
        Shell.SetTabBarIsVisible(this, true);
#else
        Shell.SetTabBarIsVisible(this, show);
#endif
        Shell.SetNavBarIsVisible(this, show);
        _viewModel.IsMaximized = !show;

        HideUnHide?.IconImageSource = show ? "maximize_2_active.png" : "minimize_2_active.png";
        UnmaximizeFab?.IsVisible = !show;
        InvalidateMeasure();
    }

    // wire the FAB
    private void UnmaximizeFab_Clicked(object? sender, EventArgs e) => SetChromeVisible(true);
    private void HideUnHideTabBar(object? sender, EventArgs e) => ToggleChrome(sender, e);

    // private void HideUnHideTabBar(object? sender, EventArgs e) {
    //     if (BindingContext is OperateViewModel viewModel) {
    //         SetTabBarState(!viewModel.IsMaximized);
    //     }
    // }

    // private void SetTabBarState(bool state) {
    //     if (state) {
    //         Shell.SetTabBarIsVisible(this, true);
    //         HideUnHide.IconImageSource = "maximize_2_active.png";
    //     } else {
    //         Shell.SetTabBarIsVisible(this, false);
    //         HideUnHide.IconImageSource = "minimize_2_active.png";
    //     }
    //     if (_viewModel is { } viewModel) viewModel.IsMaximized = state;
    //     InvalidateMeasure();
    // }
}