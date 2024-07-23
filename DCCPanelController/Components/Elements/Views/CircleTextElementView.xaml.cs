using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class CircleTextElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    
    public CircleTextElementView(TextElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}