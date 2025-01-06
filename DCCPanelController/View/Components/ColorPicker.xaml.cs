using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPicker : Popup {
    
    public Color? SelectedColor { get; init; }
    
    public ColorPicker(Color color) {
        InitializeComponent();
        SelectedColor = color;
        BindingContext = this;
    }
    
    private void CloseOnSelected(object? sender, EventArgs e) {
        Close(SelectedColor);
    }
}