using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.Views;

public partial class TextView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; init; }
    
    public TextView(IElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}