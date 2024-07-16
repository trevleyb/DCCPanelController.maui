using DCCPanelController.Components.Tracks.Base;

namespace DCCPanelController.Components.Tracks.Views;

public partial class ImageView : ContentView {

    public ITrackViewModel ViewModel { get; init; }
    
    public ImageView(ITrackViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}