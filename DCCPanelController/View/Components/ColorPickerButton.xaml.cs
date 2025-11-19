using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View.Components;

public partial class ColorPickerButton : ContentView {
    public static readonly BindableProperty          SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(ColorPickerButton), propertyChanged: OnSelectedColorChanged);
    public static readonly BindableProperty          AllowsNoColorProperty = BindableProperty.Create(nameof(AllowsNoColor), typeof(bool), typeof(ColorPickerButton), false, propertyChanged: (_, __, ___) => ((ColorPickerButton) _).OnPropertyChanged(nameof(ShowClearColorButton)));
    public static readonly BindableProperty          IsMultiValueProperty  = BindableProperty.Create(nameof(IsMultiValue), typeof(bool), typeof(ColorPickerButton), false);
    public static readonly BindableProperty          DefaultColorProperty  = BindableProperty.Create(nameof(DefaultColor), typeof(Color), typeof(ColorPickerButton), Colors.White);
    public static readonly BindableProperty          BorderColorProperty   = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ColorPickerButton), Colors.Gray);
    public static readonly BindableProperty          BorderWidthProperty   = BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(ColorPickerButton), 1);
    public static readonly BindableProperty          CornerRadiusProperty  = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(ColorPickerButton), 10);
    private                SfPopup?                  _popup;
    private                DataTemplate?             _popupTemplate;
    private                ColorPickerGrid?          _view;
    private                ColorPickerGridViewModel? _vm;

    public ColorPickerButton() {
        BindingContext = this;
        InitializeComponent();
    }

    public Color ActiveColor => SelectedColor ?? DefaultColor ?? Colors.White;
    public Color SelectedOffsetColor => AppleCrayonColors.GetContrastingTextColor(SelectedColor ?? Colors.White);
    public string SelectedColorText => SelectedColor == null ? IsMultiValue ? "-- Multiple --" : "Use Default" : AppleCrayonColors.Name(SelectedColor);
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

    private async Task EnsurePopup() {
        if (_popup != null) return;

        _vm = new ColorPickerGridViewModel(SelectedColor ?? Colors.White);
        _view = new ColorPickerGrid(_vm);
        _popupTemplate ??= new DataTemplate(() => _view);
        _popup = new SfPopup {
            ContentTemplate = _popupTemplate,
            ShowHeader = false,
            ShowFooter = false,
            BackgroundColor = Colors.Transparent,
            PopupStyle = new PopupStyle {
                CornerRadius = 10,
                HasShadow = false,
                BlurIntensity = PopupBlurIntensity.Light,
            },

            AutoSizeMode = PopupAutoSizeMode.Height,
            WidthRequest = 388,
            AnimationMode = PopupAnimationMode.None,
            AnimationDuration = 0,
        };

        // wire once
        _vm.ColorSelectionCompleted += color => {
            _popup?.Dismiss();
            if (color is { }) SelectedColor = color;
        };
        _vm.SelectionCancelled += () => _popup?.Dismiss();
    }

    private static void OnSelectedColorChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ColorPickerButton)bindable;
        control.OnPropertyChanged(nameof(ActiveColor));
        control.OnPropertyChanged(nameof(SelectedOffsetColor));
        control.OnPropertyChanged(nameof(SelectedColorText));
        control.OnPropertyChanged(nameof(ShowClearColorButton));
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
        if (_popup is null) await EnsurePopup();
        _vm!.SelectedColor = SelectedColor ?? Colors.White;
        _popup!.Show();
    }
}