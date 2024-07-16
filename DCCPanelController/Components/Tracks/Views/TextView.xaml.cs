using DCCPanelController.Components.Tracks.Base;

namespace DCCPanelController.Components.Tracks.Views;

public partial class TextView : ContentView {

    public ITrackViewModel ViewModel { get; init; }
    
    public TextView(ITrackViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}