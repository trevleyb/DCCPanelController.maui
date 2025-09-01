using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Shapes;
using Picker = Microsoft.Maui.Controls.Picker;

namespace DCCPanelController.View.Components;

public class PickerSelector : ContentView {
    private Image _clearImage = new();
    private bool _isInitialized; // Track if we've been initialized
    private Grid? _mainButtonLayout;
    private Border? _mainLayoutBox;
    private Label? _selectedItemLabel;

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

        if (SelectedItem is null && SelectedValue is not null) UpdateSelectedItem();
        if (SelectedItem is not null && SelectedValue is null) UpdateSelectedValue();

        _mainLayoutBox = new Border {
            Margin = Margin,
            Padding = Padding,
            WidthRequest = WidthRequest,
            HeightRequest = HeightRequest,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            StrokeThickness = BorderWidth,
            Stroke = BorderColor,
            StrokeShape = new RoundRectangle {
                CornerRadius = CornerRadius
            }
        };

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
        _selectedItemLabel.SetBinding(Label.TextProperty, new MultiBinding {
            Bindings = {
                new Binding(nameof(SelectedItem), source: this),
                new Binding(nameof(Placeholder), source: this),
                new Binding(nameof(DisplayMemberPath), source: this),
                new Binding(nameof(DisplayFormat), source: this)
            },
            Converter = new PickerSelectedItemToDisplayTextConverter()
        });

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
        _mainButtonLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        _mainButtonLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _mainLayoutBox.Content = _selectedItemLabel;
        _mainButtonLayout.Children.Add(_mainLayoutBox);
        _mainButtonLayout.SetColumn(_mainLayoutBox, 0);

        // The up/down image. Use properties to change what .png is used. (must be PNG)  
        // ----------------------------------------------------------------------------
        if (ShowClearFieldImage && HasItems(ItemsSource)) {
            var clearGesture = new TapGestureRecognizer();
            clearGesture.Tapped += (_, _) => SelectedItem = null;
            _clearImage = new Image {
                Source = ClearFieldImageSource,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(1, 1, 2, 1)
            };
            _clearImage.GestureRecognizers.Add(clearGesture);
            _mainButtonLayout.Children.Add(_clearImage);
            _mainButtonLayout.SetColumn(_clearImage, 1);
        }
        Content = _mainButtonLayout;
    }

    private bool HasItems(IEnumerable itemsSource) {
        return (itemsSource?.Cast<object?>()?.Count() ?? 0) > 0;
    }

    private void UpdateSelectedItem() {
        if (SelectedValue == null) SelectedItem = null;
        if (string.IsNullOrEmpty(SelectedValuePath)) {
            SelectedItem = SelectedValue;
            return;
        }

        // Find the item in ItemsSource that has the matching SelectedValue
        if (HasItems(ItemsSource)) {
            foreach (var item in ItemsSource) {
                var itemValue = GetPropertyValue(item, SelectedValuePath);
                if (Equals(itemValue, SelectedValue)) {
                    SelectedItem = item;
                    return;
                }
            }
        }
        SelectedItem = null;
    }

    private void UpdateSelectedValue() {
        if (string.IsNullOrEmpty(SelectedValuePath)) {
            SelectedValue = SelectedItem;
        } else if (SelectedItem != null) {
            SelectedValue = GetPropertyValue(SelectedItem, SelectedValuePath);
        } else {
            SelectedValue = null;
        }
    }

    /// <summary>
    ///     Retrieves the value of a specified property from the given object based on
    ///     the provided property path. If the value is null or the property path is invalid,
    ///     a default value is returned.
    /// </summary>
    /// <param name="source">The object from which the property value should be retrieved.</param>
    /// <param name="propertyPath">The path or name of the property to retrieve the value from.</param>
    /// <param name="defaultValue">The value to return if the property path is null, invalid, or the resulting value is null.</param>
    /// <returns>The value of the specified property, or the default value if the property is null or not found.</returns>
    public static object? GetPropertyValue(object? source, string? propertyPath, string? defaultValue = null) {
        if (source == null) return defaultValue;
        if (string.IsNullOrEmpty(propertyPath)) return source;
        var property = source.GetType().GetProperty(propertyPath);
        var value = property?.GetValue(source);
        return string.IsNullOrEmpty(value?.ToString()) ? defaultValue : property?.GetValue(source);
    }

    private Dictionary<string, object>? BuildDisplayItems(IEnumerable itemsSource) {
        if (!HasItems(ItemsSource)) return null;
        var displayItems = new Dictionary<string, object>();

        // Create display items for the picker based on DisplayMemberPath or DisplayFormat
        foreach (var item in ItemsSource) {
            string displayText;
            if (!string.IsNullOrEmpty(DisplayFormat)) {
                var formattedText = FormatObject(item, DisplayFormat);
                displayText = formattedText ?? item.ToString() ?? string.Empty;
            } else if (!string.IsNullOrEmpty(DisplayMemberPath)) {
                var displayValue = GetPropertyValue(item, DisplayMemberPath);
                displayText = displayValue?.ToString() ?? string.Empty;
            } else {
                displayText = item.ToString() ?? string.Empty;
            }
            displayItems.TryAdd(displayText, item);
        }
        return displayItems;
    }

    private int FindIndexOfSelectedValue(object? selectedValue, Dictionary<string, object> displayItems) {
        if (selectedValue is null) return 0;
        for (var index = 0; index < displayItems.Count; index++) {
            var displayItem = displayItems.ElementAt(index);
            if (!string.IsNullOrEmpty(SelectedValuePath)) {
                var foundValue = GetPropertyValue(displayItem.Value, SelectedValuePath);
                if (selectedValue == foundValue) return index;
            }
        }
        return 0;
    }

    private void ShowPicker() {
        ShowStandardPicker();
    }

    /// <summary>
    ///     Main Show the picker function
    /// </summary>
    private void ShowStandardPicker() {
        var displayItems = BuildDisplayItems(ItemsSource);
        if (displayItems == null) return;

        var pickerItems = displayItems.Keys.ToList();
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
        var index = FindIndexOfSelectedValue(SelectedValue, displayItems);
        picker.SelectedIndex = index;

        picker.SelectedIndexChanged += (sender, e) => {
            if (picker.SelectedIndex >= 0 && picker.SelectedIndex < displayItems.Count) {
                var selectedItem = displayItems.Values.ToArray()[picker.SelectedIndex];
                SelectedItem = selectedItem;
                UpdateSelectedValue();
            }
        };

        UpdateSelectedItem();

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

    /// <summary>
    ///     Formats an object using a format string with property placeholders like "{Name} ({Id})"
    /// </summary>
    /// <param name="source">The object to format</param>
    /// <param name="formatString">The format string with property names in curly braces</param>
    /// <returns>The formatted string, or null if source is null</returns>
    public static string? FormatObject(object? source, string formatString) {
        if (source == null) return null; // Return null so the converter can use placeholder
        if (string.IsNullOrEmpty(formatString)) return source.ToString();

        var result = formatString;
        var regex = new Regex(@"\{(\w+)\}");
        var matches = regex.Matches(formatString);

        foreach (Match match in matches) {
            var propertyName = match.Groups[1].Value;
            var propertyValue = GetPropertyValue(source, propertyName)?.ToString() ?? string.Empty;
            result = result.Replace(match.Value, propertyValue);
        }
        return result;
    }

    #region Bindable Properties
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(PickerSelector), propertyChanged: RefreshControl);
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PickerSelector), null, BindingMode.TwoWay);
    public static readonly BindableProperty SelectedValuePathProperty = BindableProperty.Create(nameof(SelectedValuePath), typeof(string), typeof(PickerSelector));
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(nameof(SelectedValue), typeof(object), typeof(PickerSelector), null, BindingMode.TwoWay);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(PickerSelector), propertyChanged: RefreshControl);
    public static readonly BindableProperty DisplayFormatProperty = BindableProperty.Create(nameof(DisplayFormat), typeof(string), typeof(PickerSelector), propertyChanged: RefreshControl);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PickerSelector), string.Empty, propertyChanged: RefreshControl);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(PickerSelector), Colors.Black, propertyChanged: RefreshControl);
    public static readonly BindableProperty TextSizeProperty = BindableProperty.Create(nameof(TextSize), typeof(double), typeof(PickerSelector), 12.0, propertyChanged: RefreshControl);

    public static readonly BindableProperty ShowClearFieldImageProperty = BindableProperty.Create(nameof(ShowClearFieldImage), typeof(bool), typeof(PickerSelector), false, propertyChanged: RefreshControl);
    public static readonly BindableProperty ClearFieldImageSourceProperty = BindableProperty.Create(nameof(ClearFieldImageSource), typeof(string), typeof(PickerSelector), "x_circle.png", propertyChanged: RefreshControl);

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(PickerSelector), new CornerRadius(10), propertyChanged: RefreshControl);
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(PickerSelector), Colors.DarkGrey, propertyChanged: RefreshControl);
    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(PickerSelector), 1.0, propertyChanged: RefreshControl);

    private static void RefreshControl(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PickerSelector selector) selector.DrawPopup();
    }

    /// <summary>
    ///     The source of the dropdown list. This is either a collection of strings
    ///     in which case Path should be null, or an object and Path should point to
    ///     the property in the object that should be displayed.
    /// </summary>
    public IEnumerable ItemsSource {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    ///     The currently selected item. If an item is clicked, this will become
    ///     the selected item.
    /// </summary>
    public object? SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
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
    ///     Part of the DropDown Configuration: How rounded should the dropdown be?
    ///     The default is 10. Set to 0 for square corners.
    /// </summary>
    public CornerRadius CornerRadius {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
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

    /// <summary>
    ///     Gets or sets the width of the border surrounding the dropdown.
    /// </summary>
    public double BorderWidth {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    /// <summary>
    ///     Gets or sets the background color of the dropdown list.
    ///     This property determines the visual background appearance
    ///     of the dropdown items when displayed.
    /// </summary>
    public Color BorderColor {
        get => (Color)GetValue(BorderColorProperty) ?? Colors.Gainsboro;
        set => SetValue(BorderColorProperty, value);
    }

    public bool ShowClearFieldImage {
        get => (bool)GetValue(ShowClearFieldImageProperty);
        set => SetValue(ShowClearFieldImageProperty, value);
    }

    /// <summary>
    ///     Gets or sets the image source used to represent the closed state of the dropdown.
    /// </summary>
    public string ClearFieldImageSource {
        get => (string)GetValue(ClearFieldImageSourceProperty);
        set => SetValue(ClearFieldImageSourceProperty, value);
    }

    /// <summary>
    ///     Gets or sets the property path that is used to get the display value for each item in the ItemsSource.
    ///     When null or empty, the item's ToString() method is used.
    /// </summary>
    public string DisplayMemberPath {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    ///     Gets or sets the property path that is used to get the value from the selected item.
    ///     When null or empty, the entire object is used as the value.
    /// </summary>
    public string SelectedValuePath {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// <summary>
    ///     Gets or sets the value that is extracted from the selected item using the SelectedValuePath.
    ///     This is what gets bound to your model property (e.g., the ID).
    /// </summary>
    public object? SelectedValue {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public string DisplayFormat {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }
    #endregion
}

public class PickerSelectedItemToDisplayTextConverter : IMultiValueConverter {
    public object Convert(object?[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values?.Length != 4) return string.Empty;

        var selectedItem = values[0];
        var placeholder = values[1] as string ?? string.Empty;
        var displayMemberPath = values[2] as string;
        var displayFormat = values[3] as string;

        if (selectedItem is null) return placeholder ?? "";

        // If we have a display format, use it
        if (!string.IsNullOrEmpty(displayFormat)) {
            return PickerSelector.FormatObject(selectedItem, displayFormat) ?? placeholder;
        }

        // If we have a display member path, use it
        if (!string.IsNullOrEmpty(displayMemberPath)) {
            var displayValue = PickerSelector.GetPropertyValue(selectedItem, displayMemberPath);
            return displayValue?.ToString() ?? placeholder;
        }

        // Default: use ToString()
        return selectedItem.ToString() ?? placeholder;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}