namespace DCCPanelController.Components.Tracks.Views;

public partial class TrackView : ContentView {

    public ITrackViewModel ViewModel { get; init; }
    
    public TrackView(ITrackViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}