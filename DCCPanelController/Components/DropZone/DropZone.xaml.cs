using DCCPanelController.ViewModel;

namespace DCCPanelController.Components.DropZone;

public partial class DropZone : ContentView {
    public DropZone(PanelEditorViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}