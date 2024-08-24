using System.ComponentModel;
using DCCPanelController.ViewModel;


namespace DCCPanelController.View;

public partial class TrackPieceTestPage : ContentPage {
    
    private readonly TrackPieceTestViewModel _viewModel;
    
    public TrackPieceTestPage() {
        InitializeComponent();
        try {
            _viewModel = new TrackPieceTestViewModel();
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            BindingContext = _viewModel;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
    
    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Property {e.PropertyName} changed from {sender?.ToString()}");
    }
}