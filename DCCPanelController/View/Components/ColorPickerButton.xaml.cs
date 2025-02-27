using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;

namespace DCCPanelController.View.Components;

public partial class ColorPickerButton : ContentView {
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorPickerButton), null, propertyChanged: SelectedColorPropertyChanged);
    public static readonly BindableProperty AllowsNoColorProperty = BindableProperty.Create(nameof(AllowsNoColor), typeof(bool), typeof(ColorPickerButton), false, propertyChanged: AllowsNoColorPropertyChanged);

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

    private static void SelectedColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ColorPickerButton)bindable;
    }

    public bool AllowsNoColor {
        get => (bool)GetValue(AllowsNoColorProperty);
        set {
            SetValue(AllowsNoColorProperty, value);
            OnPropertyChanged(nameof(AllowsNoColorProperty)); // Update DisplayText when the color changes
        }
    }

    private static void AllowsNoColorPropertyChanged(BindableObject bindable, object? oldvalue, object? newvalue) {
        var control = (ColorPickerButton)bindable;
    }
    
    // Asynchronously show the popup and update the selected color
    [RelayCommand]
    private async Task ShowDropdown() {
        var popup = new ColorPicker(SelectedColor ?? Colors.White);
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is Color selectedColor) {
                SelectedColor = selectedColor;
            }
        }
    }
}