using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class ImageElementView : ContentView, IElementView {

    public IElementViewModel ViewModel { get; set; }
    public ImageElementView(ImageElementViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
   
}