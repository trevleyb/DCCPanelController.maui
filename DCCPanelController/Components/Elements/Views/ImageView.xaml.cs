using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.Views;

public partial class ImageView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; init; }
    
    public ImageView(IElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
   
}