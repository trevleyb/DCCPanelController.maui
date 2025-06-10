using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private bool _tabBarState = true;
    private DisplayOrientation _lastOrientation;

    public OperatePage(OperateViewModel viewModel) {
        BindingContext = viewModel;
        InitializeComponent();
        _lastOrientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
        DeviceDisplay.Current.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        SetTabBarState(true);
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        if (e.DisplayInfo.Orientation != _lastOrientation) {
            _lastOrientation = e.DisplayInfo.Orientation;
            MainThread.BeginInvokeOnMainThread(() => {
                // Force EmptyView refresh
                var currentEmptyView = PanelCarousel.EmptyView;
                PanelCarousel.EmptyView = null;
                PanelCarousel.EmptyView = currentEmptyView;
            });
        }
    }
    
    private void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        if (BindingContext is OperateViewModel viewModel) {
            if (e.Tile is ITileInteractive { } tile) {
                if (e.IsSingleTap) tile.Interact(viewModel.ConnectionService);
                if (e.IsDoubleTap) tile.Secondary(viewModel.ConnectionService);
            }
        }
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