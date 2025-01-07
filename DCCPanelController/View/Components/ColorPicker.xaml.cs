using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPicker : Popup {

    private Color? _selectedColor;
    public Color? SelectedColor {
        get => _selectedColor;
        set {
            _selectedColor = value;
            OnPropertyChanged();
        }
    }
    
    public ColorPicker(Color color) {
        InitializeComponent();
        SelectedColor = color;
        BindingContext = this;
    }
    
    private async void CloseOnSelected(object? sender, EventArgs e) {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(SelectedColor, cts.Token);
    }
}