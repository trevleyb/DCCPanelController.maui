using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.Views;

public partial class TrackView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; init; }
    
    public TrackView(IElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}