using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class TrackElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    
    public TrackElementView(TrackElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}