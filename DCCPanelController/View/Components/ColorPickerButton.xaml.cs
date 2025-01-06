using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPickerButton : ContentView {
    // Property for the currently selected color
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorDropdown), Colors.White);

    public ColorPickerButton() {
        InitializeComponent();
        BindingContext = this;
    }

    public Color? SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            SetValue(SelectedColorProperty, value);
            OnPropertyChanged(nameof(SelectedColorProperty)); // Update DisplayText when the color changes
        }
    }

    // Asynchronously show the popup and update the selected color
    [RelayCommand]
    private async Task ShowDropdown() {
        var popup = new ColorPicker(SelectedColor ?? Colors.White);
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is ColorOption selectedColor) {
                SelectedColor = selectedColor.Color;
            }
        }
    }
}