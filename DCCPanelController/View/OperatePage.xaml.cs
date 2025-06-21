using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private bool _tabBarState = true;
    private DisplayOrientation? _lastOrientation;

    public OperatePage(OperateViewModel viewModel) {
        BindingContext = viewModel;
        InitializeComponent();
        _lastOrientation = null;
        DeviceDisplay.Current.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        SetTabBarState(true);
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        _lastOrientation = null;
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) {
        RefreshEmptyViewOnDisplayInfoChange(e.DisplayInfo.Orientation);
    }

    private void RefreshEmptyViewOnDisplayInfoChange(DisplayOrientation orientation) {
        if (orientation != _lastOrientation) {
            _lastOrientation = orientation;
            MainThread.BeginInvokeOnMainThread(() => {
                // Force EmptyView refresh
                var currentEmptyView = PanelCarousel.EmptyView;
                PanelCarousel.EmptyView = null;
                PanelCarousel.EmptyView = currentEmptyView;
            });
        }
    }

    private async void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        if (BindingContext is OperateViewModel viewModel) {
            if (e.Tile is ITileInteractive { } tile) {
                if (e.IsSingleTap) await tile.Interact(viewModel.ConnectionService);
                if (e.IsDoubleTap) await tile.Secondary(viewModel.ConnectionService);
            }
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