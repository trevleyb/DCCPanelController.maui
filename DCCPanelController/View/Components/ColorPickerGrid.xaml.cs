namespace DCCPanelController.View.Components;

public partial class ColorPickerGrid : ContentView {
    public ColorPickerGrid(ColorPickerGridViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}