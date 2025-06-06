using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;

namespace DCCPanelController.View.Components;

public partial class ColorPickerButton : ContentView {
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorPickerButton), propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty AllowsNoColorProperty = BindableProperty.Create(nameof(AllowsNoColor), typeof(bool), typeof(ColorPickerButton), false, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty DefaultColorProperty = BindableProperty.Create(nameof(DefaultColor), typeof(Color), typeof(ColorPickerButton), Colors.White, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ColorPickerButton), Colors.Gray, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(ColorPickerButton), 1, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(ColorPickerButton), 10, propertyChanged: ColorPropertyChanged);
    
    public ColorPickerButton() {
        InitializeComponent();
        BindingContext = this;
    }

    public Color ActiveColor => SelectedColor ?? DefaultColor ?? Colors.White;
    public string SelectedColorText => SelectedColor == null ? "Use Default" : "";
    public bool ShowClearColorButton => SelectedColor != null && AllowsNoColor;

    public Color? DefaultColor {
        get => (Color)GetValue(DefaultColorProperty);
        init {
            SetValue(DefaultColorProperty, value);
            OnPropertyChanged(nameof(DefaultColorProperty)); // Update DisplayText when the color changes
        }
    }

    public Color? SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            SetValue(SelectedColorProperty, value);
            OnPropertyChanged(nameof(SelectedColorProperty)); // Update DisplayText when the color changes
        }
    }

    public bool AllowsNoColor {
        get => (bool)GetValue(AllowsNoColorProperty);
        set {
            SetValue(AllowsNoColorProperty, value);
            OnPropertyChanged(nameof(AllowsNoColorProperty)); // Update DisplayText when the color changes
        }
    }

    public Color? BorderColor {
        get => (Color)GetValue(BorderColorProperty);
        set {
            SetValue(BorderColorProperty, value);
            OnPropertyChanged(nameof(BorderColorProperty)); // Update DisplayText when the color changes
        }
    }

    public int BorderWidth {
        get => (int)GetValue(BorderWidthProperty);
        set {
            SetValue(BorderWidthProperty, value);
            OnPropertyChanged(nameof(BorderWidthProperty)); // Update DisplayText when the color changes
        }
    }
    
    public int CornerRadius {
        get => (int)GetValue(CornerRadiusProperty);
        set {
            SetValue(CornerRadiusProperty, value);
            OnPropertyChanged(nameof(CornerRadiusProperty)); // Update DisplayText when the color changes
        }
    }

    private static void ColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ColorPickerButton)bindable;
        control.OnPropertyChanged(nameof(ActiveColor));
        control.OnPropertyChanged(nameof(SelectedColor));
        control.OnPropertyChanged(nameof(ShowClearColorButton));
        control.OnPropertyChanged(nameof(SelectedColorText));
        control.OnPropertyChanged(nameof(AllowsNoColor));
    }

    [RelayCommand]
    private async Task ClearColorAsync() {
        SelectedColor = null;
    }

    // Asynchronously show the popup and update the selected color
    [RelayCommand]
    private async Task ShowDropdownAsync() {
        var popup = new GridColorPicker(SelectedColor ?? Colors.White);
        if (App.Current?.Windows[0]?.Page is Page { } mainPage) {
            var result = await mainPage.ShowPopupAsync(popup);
            if (result is Color selectedColor) {
                SelectedColor = selectedColor;
            }
        }
    }
}