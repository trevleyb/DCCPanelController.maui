using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    
    public OperatePage() {
        InitializeComponent();
        BindingContext = new ControlPanelPageViewModel();
        PanelCarousel.CurrentItemChanged += PanelCarouselOnCurrentItemChanged;
    }

    private void PanelCarouselOnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e) {
        if (BindingContext is ControlPanelPageViewModel viewModel) {
            viewModel.SelectedPanel = PanelCarousel.CurrentItem as ControlPanelViewModel;
            Title = viewModel?.SelectedPanel?.Name ?? "Control Panel";
            OnPropertyChanged(nameof(viewModel.SelectedPanel));
        }
    }
}