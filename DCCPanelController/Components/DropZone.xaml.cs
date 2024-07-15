using DCCPanelController.ViewModel;

namespace DCCPanelController.Components;

public partial class DropZone : ContentView {
    public DropZone(PanelEditorViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}