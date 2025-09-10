using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View.Components;

public partial class ColorPickerButton : ContentView {
    private SfPopup?                  _popup;
    private DataTemplate?             _popupTemplate;
    private ColorPickerGridViewModel? _vm;
    private ColorPickerGrid?          _view;

    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorPickerButton), propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty AllowsNoColorProperty = BindableProperty.Create(nameof(AllowsNoColor), typeof(bool), typeof(ColorPickerButton), false, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty IsMultiValueProperty  = BindableProperty.Create(nameof(IsMultiValue), typeof(bool), typeof(ColorPickerButton), false, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty DefaultColorProperty  = BindableProperty.Create(nameof(DefaultColor), typeof(Color), typeof(ColorPickerButton), Colors.White, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty BorderColorProperty   = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ColorPickerButton), Colors.Gray, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty BorderWidthProperty   = BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(ColorPickerButton), 1, propertyChanged: ColorPropertyChanged);
    public static readonly BindableProperty CornerRadiusProperty  = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(ColorPickerButton), 10, propertyChanged: ColorPropertyChanged);

    public ColorPickerButton() {
        InitializeComponent();
        BindingContext = this;
    }

    public Color ActiveColor => SelectedColor ?? DefaultColor ?? Colors.White;
    public Color SelectedOffsetColor => AppleCrayonColors.GetContrastingTextColor(SelectedColor ?? Colors.White);
    public string SelectedColorText => SelectedColor == null ? (IsMultiValue ? "-- Multiple --" : "Use Default") : AppleCrayonColors.Name(SelectedColor);
    public bool ShowClearColorButton => (SelectedColor != null || IsMultiValue) && AllowsNoColor;

    public Color? DefaultColor {
        get => (Color)GetValue(DefaultColorProperty);
        set {
            SetValue(DefaultColorProperty, value);
            OnPropertyChanged(nameof(DefaultColorProperty)); // Update DisplayText when the color changes
        }
    }

    public Color? SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            SetValue(SelectedColorProperty, value);
            OnPropertyChanged(nameof(SelectedColorProperty));
            OnPropertyChanged(nameof(SelectedOffsetColor));
            OnPropertyChanged(nameof(SelectedColorText));
            OnPropertyChanged(nameof(ShowClearColorButton));
        }
    }

    public bool AllowsNoColor {
        get => (bool)GetValue(AllowsNoColorProperty);
        set {
            SetValue(AllowsNoColorProperty, value);
            OnPropertyChanged(nameof(AllowsNoColorProperty)); // Update DisplayText when the color changes
        }
    }

    public bool IsMultiValue {
        get => (bool)GetValue(IsMultiValueProperty);
        set {
            SetValue(IsMultiValueProperty, value);
            OnPropertyChanged(nameof(IsMultiValueProperty)); // Update DisplayText when the color changes
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

    protected override void OnHandlerChanged() {
        base.OnHandlerChanged();
        EnsurePopup();
    }

    private void EnsurePopup() {
        if (_popup != null) return;

        _vm = new ColorPickerGridViewModel(SelectedColor ?? Colors.White);
        _view = new ColorPickerGrid(_vm);
        _popupTemplate ??= new DataTemplate(() => _view);
        _popup = new() {
            ContentTemplate = _popupTemplate,
            ShowHeader = false,
            ShowFooter = false,
            BackgroundColor = Colors.Transparent,
            PopupStyle = new Syncfusion.Maui.Toolkit.Popup.PopupStyle {
                CornerRadius = 10,
                HasShadow = false,
                BlurIntensity = PopupBlurIntensity.Light,
            },

            AutoSizeMode = PopupAutoSizeMode.Height,
            WidthRequest = 388, 
            AnimationMode =PopupAnimationMode.None,
            AnimationDuration = 0,
        };

        // wire once
        _vm.ColorSelectionCompleted += color => {
            _popup?.Dismiss();
            if (color is { }) SelectedColor = color;
        };
        _vm.SelectionCancelled += () => _popup?.Dismiss();
        _view.Measure(double.PositiveInfinity, double.PositiveInfinity);
    }

    private static void ColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ColorPickerButton)bindable;
        control.OnPropertyChanged(nameof(ActiveColor));
        control.OnPropertyChanged(nameof(SelectedColor));
        control.OnPropertyChanged(nameof(ShowClearColorButton));
        control.OnPropertyChanged(nameof(SelectedColorText));
        control.OnPropertyChanged(nameof(AllowsNoColor));
        control.OnPropertyChanged(nameof(SelectedColorProperty));
        control.OnPropertyChanged(nameof(SelectedOffsetColor));
    }

    [RelayCommand]
    private async Task ClearColorAsync() {
        IsMultiValue = false;
        SelectedColor = null;
        OnPropertyChanged(nameof(SelectedColorProperty));
        OnPropertyChanged(nameof(SelectedOffsetColor));
        OnPropertyChanged(nameof(SelectedColorText));
        OnPropertyChanged(nameof(ShowClearColorButton));
    }

    [RelayCommand]
    private async Task ShowDropdownAsync() {
        EnsurePopup();
        _vm?.SelectedColor = SelectedColor ?? Colors.White;
        _popup?.Show();
    }
}