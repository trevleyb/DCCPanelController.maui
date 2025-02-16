using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorGridDropdown : ContentView {

    // Property for the currently selected color
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorDropdown), Colors.White, propertyChanged: ColorPropertyChanged);
    private ColorOption _selectedColorOption = PredefinedColors.Default;

    public ColorGridDropdown() {
        InitializeComponent();
        ShowDropdownCommand = new Command(ShowDropdown);
        BindingContext = this;
    }

    public ICommand ShowDropdownCommand { get; set; }

    public Color SelectedColorContrast => _selectedColorOption.ContrastColor;
    public string SelectedColorName => _selectedColorOption.Name ?? "None";

    public Color? SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            _selectedColorOption = value != null ? PredefinedColors.FindColor(value) : PredefinedColors.None;
            SetValue(SelectedColorProperty, _selectedColorOption.Color);
            OnPropertyChanged(nameof(SelectedColorProperty)); // Update DisplayText when the color changes
            OnPropertyChanged(nameof(SelectedColorName));     // Update DisplayText when the color changes
            OnPropertyChanged(nameof(SelectedColorContrast)); // Update DisplayText when the color changes
        }
    }

    private static void ColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        //if (bindable is ColorGridDropdown control)
        //    if (newvalue is Color { } color) {
        //        control._selectedColorOption = PredefinedColors.FindColor(color) ?? PredefinedColors.None;
        //    } 
        //    else control._selectedColorOption = PredefinedColors.None;
    }

    // Asynchronously show the popup and update the selected color
    private async void ShowDropdown() {
        var popup = new ColorGrid();
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is ColorOption selectedColor) {
                SelectedColor = selectedColor.Color;
            } else SelectedColor = null;
        }
    }
}