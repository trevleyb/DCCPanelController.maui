using Microsoft.Maui.Controls.Shapes;
using Picker = Microsoft.Maui.Controls.Picker;

namespace DCCPanelController.View.Actions;

public class ActionPickerSelector : ContentView {
    private Image   _clearImage = new();
    private bool    _isInitialized; // Track if we've been initialized
    private Grid?   _mainButtonLayout;
    private Label?  _selectedItemLabel;

    protected override void OnHandlerChanged() {
        base.OnHandlerChanged();
        if (Handler != null) {
            _isInitialized = true;
            DrawPopup();
        }
    }

    /// <summary>
    ///     Renders the dropdown menu, handling its visual update and ensuring
    ///     that it is properly displayed within the parent container.
    /// </summary>
    public void DrawPopup() {
        if (!_isInitialized) return; // Already drawn
        
        // The label that will be displayed containing the selected item
        // ----------------------------------------------------------------------------
        _selectedItemLabel = new Label {
            BackgroundColor = BackgroundColor,
            WidthRequest = WidthRequest,
            HeightRequest = HeightRequest,
            VerticalOptions = LayoutOptions.Fill,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            LineBreakMode = LineBreakMode.TailTruncation,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(10, 5, 5, 5),
            Padding = new Thickness(5, 0, 0, 0),
            BindingContext = this
        };
        _selectedItemLabel.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), BindingMode.OneWay, source: this));
        _selectedItemLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), BindingMode.OneWay, source: this));
        _selectedItemLabel.SetBinding(Label.TextProperty, new Binding(nameof(SelectedValue), BindingMode.TwoWay, source: this));

        // Add a tap gesture to the label to show the picker
        // ----------------------------------------------------------------------------
        var pickGesture = new TapGestureRecognizer();
        pickGesture.Tapped += (_, _) => { ShowPicker(); };
        _selectedItemLabel.GestureRecognizers.Add(pickGesture);

        // Main container for the label and icon
        // ----------------------------------------------------------------------------
        _mainButtonLayout = new Grid {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
        _mainButtonLayout.Children.Add(_selectedItemLabel);
        Content = _mainButtonLayout;
    }

    private bool HasItems(List<string>? itemsSource) {
        return(itemsSource?.Count ?? 0) > 0;
    }

    private void ShowPicker() {
        ShowStandardPicker();
    }

    /// <summary>
    ///     Main Show the picker function
    /// </summary>
    private void ShowStandardPicker() {
        var pickerItems = ViewModel?.GetSelectableItems(SelectedValue) ?? [];
        var picker = new Picker {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            WidthRequest = WidthRequest,
            Margin = new Thickness(10, 0, 0, 0),
            ItemsSource = pickerItems, // Use formatted display items
            #if !MACCATALYST
            // Title is not supported on macOS
            Title = Placeholder ?? "Select an option",
            #endif
            IsVisible = false
        };
        var index = FindIndexOfSelectedValue(SelectedValue, pickerItems);
        picker.SelectedIndex = index;

        picker.SelectedIndexChanged += (sender, e) => {
            //if (picker.SelectedIndex >= 0 && picker.SelectedIndex < pickerItems.Count) {
            //    var selectedItem = pickerItems[picker.SelectedIndex];
            //}
        };

        // Add picker to the layout temporarily to show it
        if (Parent is Layout layout) {
            layout.Children.Add(picker);
            picker.Focus();

            // Remove picker after selection or when focus is lost
            picker.Unfocused += (sender, e) => {
                if (layout.Children.Contains(picker)) {
                    layout.Children.Remove(picker);
                }
            };
        }
    }
    
    private int FindIndexOfSelectedValue(string? selectedValue, List<string> displayItems) {
        if (selectedValue is null) return 0;
        for (var index = 0; index < displayItems.Count; index++) {
            if (displayItems[index] == selectedValue) return index;
        }
        return 0;
    }

    #region Bindable Properties
    public static readonly BindableProperty ViewModelProperty     = BindableProperty.Create(nameof(ViewModel), typeof(IActionsGridViewModel), typeof(ActionPickerSelector), null, BindingMode.OneWayToSource, propertyChanged: ViewModelUpdated);
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(nameof(SelectedValue), typeof(string), typeof(ActionPickerSelector),"", BindingMode.TwoWay, propertyChanged: RefreshControl);

    public static readonly BindableProperty PlaceholderProperty     = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ActionPickerSelector), string.Empty, propertyChanged: RefreshControl);
    public static readonly BindableProperty TextColorProperty       = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ActionPickerSelector), Colors.Black, propertyChanged: RefreshControl);
    public static readonly BindableProperty TextSizeProperty        = BindableProperty.Create(nameof(TextSize), typeof(double), typeof(ActionPickerSelector), 12.0, propertyChanged: RefreshControl);
    
    private static void RefreshControl(BindableObject bindable, object? oldValue, object? newValue) {
        if (bindable is ActionPickerSelector selector) {
            selector.DrawPopup();
        }
    }

    private static void ViewModelUpdated(BindableObject bindable, object oldValue, object newValue) { }
    
    public IActionsGridViewModel? ViewModel {
        get => (IActionsGridViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    /// <summary>
    ///     Gets or sets the value that is extracted from the selected item using the SelectedValuePath.
    ///     This is what gets bound to your model property (e.g., the ID).
    /// </summary>
    public string SelectedValue {
        get => (string)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }
    
    /// <summary>
    ///     If the current object is blank, this is the text that should be
    ///     displayed as a placeholder.
    /// </summary>
    public string Placeholder {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    ///     Gets or sets the color of the text within the DropDownBox.
    /// </summary>
    public Color TextColor {
        get => (Color)GetValue(TextColorProperty) ?? Colors.Black;
        set => SetValue(TextColorProperty, value);
    }

    /// <summary>
    ///     Determines the font size of the text displayed in the drop-down box.
    /// </summary>
    public double TextSize {
        get => (double)GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }

    #endregion
}