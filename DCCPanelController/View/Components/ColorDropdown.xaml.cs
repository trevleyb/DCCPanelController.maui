using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorDropdown : ContentView {
    private const string DefaultPlaceholderText = "Click here to select a color";

    // Property for the currently selected color
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorDropdown), Colors.White, propertyChanged: ColorPropertyChanged);

    private static void ColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ColorDropdown)bindable; 
        control.OnPropertyChanged(nameof(DisplayText));           // Update DisplayText when the color changes
        control.OnPropertyChanged(nameof(ContrastColor));          // Update DisplayText when the color changes
    }

    public ColorDropdown() {
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
            OnPropertyChanged(nameof(DisplayText));           // Update DisplayText when the color changes
            OnPropertyChanged(nameof(ContrastColor));         // Update DisplayText when the color changes
        }
    }

    public string DisplayText => SelectedColor?.ColorName() ?? DefaultPlaceholderText;
    public Color ContrastColor => SelectedColor.ToInverseColor();

    // Asynchronously show the popup and update the selected color
    private async void ShowDropdown() {
        var popup = new ColorPopup();
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is ColorOption selectedColor) {
                SelectedColor = selectedColor.Color;
            }
        }
    }

    public static bool IsColorDark(Color color) {
        // Using relative luminance to determine if the color is dark or light
        double brightness = (color.Red * 255 * 299 + color.Green * 255 * 587 + color.Blue * 255 * 114) / 1000;
        return brightness < 128;
    }

    public static Color GetContrastingColor(Color color) {
        return IsColorDark(color) ? Colors.White : Colors.Black;
    }
}