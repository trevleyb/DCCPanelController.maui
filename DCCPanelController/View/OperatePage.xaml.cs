using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
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
    
    private void PanelView_OnTrackPieceTapped(object? sender, ITrackPiece e) {
        Console.WriteLine($"In Operate Mode: Track {e.Name} was tapped");
        if (e is not null and ITrackInteractive trackPieceTapped) {
            trackPieceTapped.Clicked();
        }
    }
    
    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

}