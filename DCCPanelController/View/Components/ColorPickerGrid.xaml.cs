using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPickerGrid : ContentView {
    
    public ColorPickerGrid(ColorPickerGridViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void ColorsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        Console.WriteLine("Selection changed");
    }
}