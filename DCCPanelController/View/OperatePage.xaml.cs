using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class OperatePage : ContentPage, INotifyPropertyChanged {
    
    public OperatePage() {
        InitializeComponent();
        BindingContext = new OperateViewModel();
        PanelCarousel.CurrentItemChanged += PanelCarouselOnCurrentItemChanged;
    }

    private void PanelCarouselOnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e) {
        if (BindingContext is OperateViewModel viewModel) {
            Title = viewModel.SetActivePanel(PanelCarousel.CurrentItem as Panel);
            OnPropertyChanged(nameof(viewModel.SelectedPanel));
        }
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }
}