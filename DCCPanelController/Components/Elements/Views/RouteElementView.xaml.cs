using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class RouteElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    
    public RouteElementView(RouteElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}