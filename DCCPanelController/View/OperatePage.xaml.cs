using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    private bool _tabBarState = true;

    public OperatePage(OperateViewModel viewModel) {
        BindingContext = viewModel;
        InitializeComponent();
        //PanelCarousel.CurrentItemChanged += PanelCarouselOnCurrentItemChanged;
        SetTabBarState(true);
    }

    // private void PanelCarouselOnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e) {
    //     Console.WriteLine($"Panel Carousel Item Changed. Sender={sender?.GetType()}");
    //     if (BindingContext is OperateViewModel viewModel) {
    //         //Title = viewModel.SetActivePanel(PanelCarousel.CurrentItem as Panel);
    //         OnPropertyChanged(nameof(viewModel.SelectedPanel));
    //     }
    // }

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

            //IndicatorView.IsVisible = true;
            HideUnHide.IconImageSource = "maximize_2.png";
        } else {
            Shell.SetTabBarIsVisible(this, false);

            //IndicatorView.IsVisible = false;
            HideUnHide.IconImageSource = "minimize_2.png";
        }

        _tabBarState = state;
    }
}