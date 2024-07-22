using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class TextElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    
    public TextElementView(TextElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}