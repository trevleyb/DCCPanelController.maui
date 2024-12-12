using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorGridDropdown : ContentView {
    private ColorOption _selectedColorOption = PredefinedColors.Default;
    
    // Property for the currently selected color
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorDropdown), Colors.White, propertyChanged: ColorPropertyChanged);

    private static void ColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = bindable as ColorGridDropdown;
    }

    public ColorGridDropdown() {
        InitializeComponent();
        ShowDropdownCommand = new Command(ShowDropdown);
        BindingContext = this;
    }

    public ICommand ShowDropdownCommand { get; set; }

    public Color SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            SetValue(SelectedColorProperty, value);
            OnPropertyChanged(nameof(SelectedColorProperty)); // Update DisplayText when the color changes
        }
    }

    // Asynchronously show the popup and update the selected color
    private async void ShowDropdown() {
        var popup = new ColorGrid();
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is ColorOption selectedColor) {
                SelectedColor = selectedColor.Color;
            }
        }
    }
}