using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class ButtonElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    
    public ButtonElementView(ButtonElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}