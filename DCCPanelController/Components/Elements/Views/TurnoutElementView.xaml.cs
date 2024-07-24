using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class TurnoutElementView : ContentView, IElementView {
    
    public IElementViewModel ViewModel { get; set; }
    
    public TurnoutElementView(TurnoutElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}