using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.View;

public partial class PanelControllerPage : ContentPage, INotifyPropertyChanged {
    public PanelControllerPage() {
        InitializeComponent();
        BindingContext = new PanelControllerPageViewModel();
    }

    private PanelControllerPageViewModel ViewModel => (PanelControllerPageViewModel)BindingContext;
    
    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private void ShowGrid(object? sender, EventArgs e) {
        ViewModel.ShowGrid = !ViewModel.ShowGrid;        
    }

    private void IncZoom(object? sender, EventArgs e) {
        if (ViewModel.ZoomFactor > 2.25) return;
        ViewModel.ZoomFactor += 0.25;
    }

    private void DecZoom(object? sender, EventArgs e) {
        if (ViewModel.ZoomFactor < 0.50) return;
        ViewModel.ZoomFactor -= 0.25;
    }

    private void Refresh(object? sender, EventArgs e) {
        var tempPanel = ViewModel.SelectedPanel;
        ViewModel.SelectedPanel = null;
        ViewModel.SelectedPanel = tempPanel;
    }

    private void TrackPieceChanged(object? sender, ITrack e) {
        Console.WriteLine($"Track Piece Changed: {e.Name}");
    }

    private void TrackPieceDoubleTapped(object? sender, ITrack e) {
        Console.WriteLine($"Track Piece Double-Tapped: {e.Name}");
    }

    private void TrackPieceTapped(object? sender, ITrack e) {
        Console.WriteLine($"Track Piece Tapped: {e.Name}");
    }
}