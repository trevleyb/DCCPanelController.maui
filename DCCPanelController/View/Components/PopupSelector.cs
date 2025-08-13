using System.Collections;
using System.Globalization;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using Application = Microsoft.Maui.Controls.Application;
using LayoutAlignment = Microsoft.Maui.Primitives.LayoutAlignment;
using ListView = Microsoft.Maui.Controls.ListView;
using Picker = Microsoft.Maui.Controls.Picker;
using VisualElement = Microsoft.Maui.Controls.VisualElement;

namespace DCCPanelController.View.Components;

public class PopupSelectorEventArgs(object? currentItem) : EventArgs {
    public object? CurrentItem { get; init; } = currentItem;
}

public enum PopupSelectorTypeEnum { Popup, Dropdown, Picker, Automatic }

public class PopupSelector : ContentView, IDisposable {
    private bool _disposed;
    private Popup? _popup;
    private Image _arrowImage = new();
    private Image _clearImage = new();
    private readonly Border _popupContainer = new();
    private bool _isLoaded = false;
    private bool _isInitializing = false;
    private Border? mainLayoutBox;
    private Grid? mainButtonLayout;
    private Label? selectedItemLabel;

    public event EventHandler<PopupSelectorEventArgs>? OnPopup;
    public event EventHandler<PopupSelectorEventArgs>? OnClosing;

    public PopupSelector() {
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e) {
        if (!_isLoaded) {
            _isLoaded = true;
            DrawPopup();
            Loaded -= OnLoaded; // Unsubscribe to prevent multiple calls
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Renders the dropdown menu, handling its visual update and ensuring
    ///     that it is properly displayed within the parent container.
    /// </summary>
    private void DrawPopup() {
        if (Handler == null) {
            Loaded += (s, e) => DrawPopup();
            return;
        }

        mainLayoutBox = new Border() {
            Margin = new Thickness(0, 0,0, 0),
            WidthRequest = this.WidthRequest,
            HeightRequest = 30,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeShape = new RoundRectangle {
                CornerRadius = new CornerRadius(10) // All corners rounded with radius 10
            }
        };

        // The label that will be displayed containing the selected item
        // ----------------------------------------------------------------------------
        selectedItemLabel = new Label {
            VerticalOptions = LayoutOptions.Fill,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            LineBreakMode = LineBreakMode.TailTruncation,
            Margin = new Thickness(10, 5, 5, 5),
        };
        selectedItemLabel.BindingContext = this;
        selectedItemLabel.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), BindingMode.OneWay, source: this));
        selectedItemLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), BindingMode.OneWay, source: this));

        //selectedItemLabel.SetBinding(Label.TextProperty, new MultiBinding {
        //    Bindings = {
        //        new Binding(nameof(SelectedItem), source: this),
        //        new Binding(nameof(Placeholder), source: this)
        //    },
        //    Converter = new SelectedItemToDisplayTextConverter()
        //});
        selectedItemLabel.SetBinding(Label.TextProperty, new MultiBinding {
            Bindings = {
                new Binding(nameof(SelectedItem), source: this),
                new Binding(nameof(Placeholder), source: this),
                new Binding(nameof(DisplayMemberPath), source: this),
                new Binding(nameof(DisplayFormat), source: this)
            },
            Converter = new SelectedItemToDisplayTextConverter()
        });

        // The up/down image. Use properties to change what .png is used. (must be PNG)  
        // ----------------------------------------------------------------------------
        _arrowImage = new Image {
            Source = DropdownClosedImageSource,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(1, 1, 1, 1)
        };
        var togglePopupGesture = new TapGestureRecognizer();
        togglePopupGesture.Tapped += (_, _) => TogglePopup();
        selectedItemLabel.GestureRecognizers.Add(togglePopupGesture);
        _arrowImage.GestureRecognizers.Add(togglePopupGesture);

        // The up/down image. Use properties to change what .png is used. (must be PNG)  
        // ----------------------------------------------------------------------------
        _clearImage = new Image {
            Source = DropdownClearFieldImageSource,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(1, 1, 1, 1)
        };
        var clearGesture = new TapGestureRecognizer();
        clearGesture.Tapped += (_, _) => SelectedItem = null;
        _clearImage.GestureRecognizers.Add(clearGesture);

        // Main container for the label and icon
        // ----------------------------------------------------------------------------
        mainButtonLayout = new Grid {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };
        mainButtonLayout.SizeChanged += (_, _) => {
            var dropdownWidth = DropDownWidth > 0 ? DropDownWidth - 2 : mainButtonLayout.Width - 2;
            AbsoluteLayout.SetLayoutBounds(_popupContainer, new Rect(0, mainButtonLayout.Height, dropdownWidth, DropDownHeight));
        };

        mainButtonLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        mainButtonLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        mainLayoutBox.Content = selectedItemLabel;

        mainButtonLayout.Children.Add(mainLayoutBox);
        mainButtonLayout.SetColumn(mainLayoutBox, 0);
        if (ShowClearFieldImage) {
            mainButtonLayout.Children.Add(_clearImage);
            mainButtonLayout.SetColumn(_clearImage, 1);
        }
        CreatePopupView(mainButtonLayout);

        // Placeholder management
        // ----------------------------------------------------------------------------
        PropertyChanged += (_, e) => {
            switch (e.PropertyName) {
            case nameof(SelectedItem):
                UpdateSelectedValue();
                break;

            case nameof(SelectedValue):
                UpdateSelectedItemFromValue();
                break;
            
            case nameof(ItemsSource):
                // When ItemsSource changes, try to restore the selection
                if (!_isInitializing && SelectedValue != null && SelectedItem == null) {
                    UpdateSelectedItemFromValue();
                }
                break;

            case nameof(DropdownSeparator):
                OnPropertyChanged(nameof(DropdownSeparatorVisibility));
                break;

            case nameof(DropdownImageTint):
                SetDropDownImage(_popupContainer.IsVisible);
                break;

            case nameof(DropdownShadow):
                if (DropdownShadow) {
                    _popupContainer.Shadow = new Shadow {
                        Opacity = 0.25f,
                        Offset = new Point(5, 5),
                        Radius = 10
                    };
                } else _popupContainer.Shadow = null!;
                break;
            }
        };

        // Complete initialization after a short delay to ensure all bindings are set
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () => {
            CompleteInitialization();
            return false; // Don't repeat
        });

    }

    // Add this method to be called after control is fully initialized
    private void CompleteInitialization() {
        _isInitializing = false;
        // Force update of SelectedItem from SelectedValue now that everything is ready
        if (SelectedValue != null && SelectedItem == null) {
            UpdateSelectedItemFromValue();
        }
    }

    private void UpdateSelectedItemFromValue() {
        // Don't update during initialization to avoid timing issues
        if (_isInitializing) return;

        if (SelectedValue == null) {
            SelectedItem = null;
            return;
        }

        if (string.IsNullOrEmpty(SelectedValuePath)) {
            SelectedItem = SelectedValue;
            return;
        }

        // Find the item in ItemsSource that has the matching SelectedValue
        if (ItemsSource != null) {
            foreach (var item in ItemsSource) {
                var itemValue = GetPropertyValue(item, SelectedValuePath);
                if (Equals(itemValue, SelectedValue)) {
                    SelectedItem = item;
                    return;
                }
            }
        }

        // Keep SelectedItem as null but don't clear SelectedValue
        SelectedItem = null;
    }

    private void UpdateSelectedValue() {
        // Don't update during initialization
        if (_isInitializing) return;

        if (string.IsNullOrEmpty(SelectedValuePath)) {
            SelectedValue = SelectedItem;
        } else if (SelectedItem != null) {
            SelectedValue = GetPropertyValue(SelectedItem, SelectedValuePath);
        } else {
            SelectedValue = null;
        }
    }

    private void UpdateSelectedItemFromValueWhenItemsSourceChanges() {
        // When ItemsSource changes, try to find the matching item again
        if (SelectedValue != null && SelectedItem == null) {
            UpdateSelectedItemFromValue();
        }
    }

    private void CreatePopupView(Microsoft.Maui.Controls.View mainButtonLayout) {
        // This list of items as a list view
        // ----------------------------------------------------------------------------
        var itemListView = new ListView {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(5),
            SelectionMode = ListViewSelectionMode.None,
            IsPullToRefreshEnabled = false,
            ItemsSource = ItemsSource,
            ItemTemplate = new DataTemplate(() => {
                var label = new Label {
                    Margin = new Thickness(5, 0, 5, 0),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Fill
                };
                label.SetBinding(Label.TextColorProperty, new Binding(nameof(DropdownTextColor), BindingMode.OneWay, source: this));
                label.SetBinding(Label.FontSizeProperty, new Binding(nameof(TextSize), BindingMode.OneWay, source: this));
                label.SetBinding(BackgroundColorProperty, new Binding(nameof(DropdownBackgroundColor), BindingMode.OneWay, source: this));

                // Check if DisplayMemberPath is set
                if (!string.IsNullOrEmpty(DisplayMemberPath)) {
                    // Use the specified property path
                    label.SetBinding(Label.TextProperty, new Binding(DisplayMemberPath));
                } else {
                    // Default behavior: bind to the entire object (uses ToString())
                    label.SetBinding(Label.TextProperty, new Binding("."));
                }

                return label;
            })
        };
        itemListView.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ItemsSource), BindingMode.TwoWay, source: this));
        itemListView.SetBinding(BackgroundColorProperty, new Binding(nameof(DropdownBackgroundColor), BindingMode.OneWay, source: this));
        itemListView.SetBinding(ListView.SeparatorColorProperty, new Binding(nameof(DropdownTextColor), BindingMode.OneWay, source: this));
        itemListView.SetBinding(ListView.SeparatorVisibilityProperty, new Binding(nameof(DropdownSeparatorVisibility), BindingMode.OneWay, source: this));
        itemListView.On<iOS>().SetSeparatorStyle(SeparatorStyle.FullWidth);
        itemListView.ItemTapped += (_, e) => {
            if (e?.Item is { } item) {
                SelectedItem = item;
                TogglePopup();
            }
        };

        // Popup container with a shadow and border
        // ----------------------------------------------------------------------------
        _popupContainer.Content = itemListView;
        _popupContainer.IsVisible = false;
        _popupContainer.Margin = new Thickness(1);
        _popupContainer.Padding = new Thickness(1);
        _popupContainer.SetBinding(BackgroundColorProperty, new Binding(nameof(DropdownBackgroundColor), BindingMode.OneWay, source: this));
        _popupContainer.SetBinding(Border.StrokeProperty, new Binding(nameof(DropdownBorderColorBrush), BindingMode.OneWay, source: this));
        _popupContainer.SetBinding(Border.StrokeThicknessProperty, new Binding(nameof(DropdownBorderWidth), BindingMode.OneWay, source: this));

        _popupContainer.Unfocused += (_, _) => {
            _popupContainer.IsVisible = false;
            SetDropDownImage(false);
        };

        // AbsoluteLayout to allow overlay over other content
        // ----------------------------------------------------------------------------
        if (SelectorType == PopupSelectorTypeEnum.Dropdown) {
            var absoluteLayout = new AbsoluteLayout();
            AbsoluteLayout.SetLayoutBounds(mainButtonLayout, new Rect(0, 0, 1, 40)); // Layout button at (0, 0)
            AbsoluteLayout.SetLayoutFlags(mainButtonLayout, AbsoluteLayoutFlags.WidthProportional);
            absoluteLayout.Children.Add(mainButtonLayout);

            AbsoluteLayout.SetLayoutBounds(_popupContainer, new Rect(0, 40, DropDownWidth > 0 ? DropDownWidth - 2 : WidthRequest - 2, DropDownHeight));
            AbsoluteLayout.SetLayoutFlags(_popupContainer, AbsoluteLayoutFlags.None);
            absoluteLayout.Children.Add(_popupContainer);
            Content = absoluteLayout;
        } else {
            Content = mainButtonLayout;
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

    private static void SelectorTypeChanged(BindableObject bindable, object oldValue, object newValue) {
        try {
            if (bindable is PopupSelector container) container.DrawPopup();
        } catch (Exception e) {
            Console.WriteLine($"Unable to change the selectoir tyep: {e.Message}");
        }
    }

    private static void CornerRadiusChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PopupSelector container) container.UpdateCornerRadius();
    }

    /// <summary>
    ///     Updates the corner radius of the dropdown's popup container to match the specified
    ///     value in the `DropdownCornerRadius` property, ensuring the visual shape of the popup
    ///     is consistent with the designated styling.
    ///     This is needed as we can't bind to the Stroke in the dropdown.
    /// </summary>
    private void UpdateCornerRadius() {
        ArgumentNullException.ThrowIfNull(_popupContainer);
        _popupContainer.StrokeShape = new RoundRectangle { CornerRadius = DropdownCornerRadius };
    }

    /// <summary>
    ///     Updates the dropdown arrow image to reflect its open or closed state.
    /// </summary>
    /// <param name="isOpen">A boolean value indicating whether the dropdown is open (true) or closed (false).</param>
    private void SetDropDownImage(bool isOpen) {
        try {
            if (isOpen) OnPopup?.Invoke(this, new PopupSelectorEventArgs(SelectedItem));
            if (!isOpen) OnClosing?.Invoke(this, new PopupSelectorEventArgs(SelectedItem));
            _arrowImage.Source = isOpen ? DropdownOpenImageSource : DropdownClosedImageSource;
            if (DropdownImageTint != null) {
                _arrowImage.Behaviors.Add(new IconTintColorBehavior { TintColor = DropdownImageTint });
            }
        } catch {
            // "Error setting dropdown image source: {(isOpen ? DropdownOpenImageSource : DropdownClosedImageSource)}");
        }
    }

    /// <summary>
    ///     Toggles the visibility of the dropdown popup.
    ///     Depending on the configured dropdown style, either shows or hides the popup
    ///     container by updating its visibility or dynamically creating and displaying a popup.
    /// </summary>
    private void TogglePopup() {
        if (SelectorType == PopupSelectorTypeEnum.Dropdown) {
            _popupContainer.IsVisible = !_popupContainer.IsVisible;
            SetDropDownImage(_popupContainer.IsVisible);
        } else {
            if (SelectorType == PopupSelectorTypeEnum.Picker || (SelectorType == PopupSelectorTypeEnum.Automatic && DeviceInfo.Platform == DevicePlatform.iOS)) {
                SetDropDownImage(true);
                ShowIOSPicker();
                SetDropDownImage(false);
                return;
            }

            // This is a normal Popup then. 
            // ----------------------------------------------------------
            if (_popup?.Parent != null) {
                if (_popup is not null) {
                    //TODO: FIX
                    //_popup.Close();
                    _popup = null;
                }
                SetDropDownImage(false);
                _popupContainer.IsVisible = false;
            } else {
                // Create and show popup
                SetDropDownImage(true);
                var bounds = GetControlBounds();
                _popupContainer.WidthRequest = DropDownWidth > 0 ? DropDownWidth : bounds.Width;
                _popupContainer.HeightRequest = DropDownHeight > 0 ? DropDownHeight : bounds.Height;
                _popup = new Popup {
                    Content = _popupContainer,
                    //TODO: FIX
                    //Size = new Size(_popupContainer.WidthRequest + InnerMargin.Left + InnerMargin.Right, DropDownHeight + InnerMargin.Top + InnerMargin.Bottom),
                    CanBeDismissedByTappingOutsideOfPopup = true,
                    //TODO: FIXVerticalOptions = LayoutAlignment.Center,
                    //TODO: FIXHorizontalOptions = LayoutAlignment.Center
                };
                if (ShowAnchor) {
                    //TODO: FIX
                    //_popup.Anchor = this;
                    _popupContainer.Margin = InnerMargin;
                }
                //TODO: FIX
                //_popup.SetBinding(Popup.ColorProperty, new Binding(nameof(DropdownBackgroundColor), BindingMode.OneWay, source: this));
                _popup.Closed += (sender, args) => {
                    SetDropDownImage(false);
                    _popupContainer.IsVisible = false;
                    _popup = null;
                };

                if (App.Current.Windows[0].Page is { } page) {
                    //TODO: FIX
                    //page.ShowPopup(_popup);
                    _popupContainer.IsVisible = true;
                }
            }
        }
    }

    void ShowIOSPicker() {
        if (ItemsSource == null) return;

        var items = ItemsSource.Cast<object>().ToList();
        if (items.Count == 0) return;

        // Create display items for the picker based on DisplayMemberPath or DisplayFormat
        var displayItems = new List<string>();
        foreach (var item in items) {
            string displayText;

            if (!string.IsNullOrEmpty(DisplayFormat)) {
                // Use the display format template
                var formattedText = FormatObject(item, DisplayFormat);
                displayText = formattedText ?? item.ToString() ?? string.Empty;
            } else if (!string.IsNullOrEmpty(DisplayMemberPath)) {
                // Use the display member path
                var displayValue = GetPropertyValue(item, DisplayMemberPath);
                displayText = displayValue?.ToString() ?? string.Empty;
            } else {
                // Default behavior: use the item's ToString() method
                displayText = item.ToString() ?? string.Empty;
            }

            displayItems.Add(displayText);
        }

        var picker = new Picker {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            WidthRequest = WidthRequest,
            Margin = new Thickness(10, 0, 0, 0),
            ItemsSource = displayItems, // Use formatted display items
            Title = Placeholder ?? "Select an option",
            IsVisible = false
        };

        // Set the selected index based on the current SelectedItem
        if (SelectedItem != null) {
            var selectedIndex = items.IndexOf(SelectedItem);
            if (selectedIndex >= 0) {
                picker.SelectedIndex = selectedIndex;
            }
        }

        picker.SelectedIndexChanged += (sender, e) => {
            if (picker.SelectedIndex >= 0 && picker.SelectedIndex < items.Count) {
                var selectedItem = items[picker.SelectedIndex];
                SelectedItem = selectedItem;

                // Update SelectedValue based on SelectedValuePath
                UpdateSelectedValue();
            }
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

    /// <summary>
    /// Formats an object using a format string with property placeholders like "{Name} ({Id})"
    /// </summary>
    /// <param name="source">The object to format</param>
    /// <param name="formatString">The format string with property names in curly braces</param>
    /// <returns>The formatted string, or null if source is null</returns>
    public static string? FormatObject(object? source, string formatString) {
        if (source == null) {
            return null; // Return null so the converter can use placeholder
        }

        if (string.IsNullOrEmpty(formatString)) {
            return source.ToString();
        }

        var result = formatString;

        // Find all property placeholders like {PropertyName}
        var regex = new System.Text.RegularExpressions.Regex(@"\{(\w+)\}");
        var matches = regex.Matches(formatString);

        foreach (System.Text.RegularExpressions.Match match in matches) {
            var propertyName = match.Groups[1].Value;
            var propertyValue = GetPropertyValue(source, propertyName)?.ToString() ?? string.Empty;
            result = result.Replace(match.Value, propertyValue);
        }

        return result;
    }

    private Rect GetControlBounds() {
        var element = this;
        var x = element.X;
        var y = element.Y;

        // Get absolute position by walking up the visual tree
        var parent = element.Parent as VisualElement;
        while (parent != null) {
            x += parent.X;
            y += parent.Y;
            parent = parent.Parent as VisualElement;
        }

        return new Rect(x, y, Width, Height);
    }

    protected void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                if (_popup is not null) {
                    //TODO: FIX
                    //_popup?.Close();
                    _popup = null;
                }
            }
            _disposed = true;
        }
    }

    #region Bindable Properties
    public static readonly BindableProperty SelectorTypeProperty = BindableProperty.Create(nameof(SelectorType), typeof(PopupSelectorTypeEnum), typeof(PopupSelector), PopupSelectorTypeEnum.Popup, propertyChanged: SelectorTypeChanged);
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(PopupSelector), propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PopupSelector), null, BindingMode.TwoWay);
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PopupSelector), string.Empty);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(PopupSelector), Colors.Black);
    public static readonly BindableProperty TextSizeProperty = BindableProperty.Create(nameof(TextSize), typeof(double), typeof(PopupSelector), 12.0);
    public static readonly BindableProperty ShowAnchorProperty = BindableProperty.Create(nameof(ShowAnchor), typeof(bool), typeof(PopupSelector), true);
    public static readonly BindableProperty InnerMarginProperty = BindableProperty.Create(nameof(InnerMargin), typeof(Thickness), typeof(PopupSelector), new Thickness(0));

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(PopupSelector));
    public static readonly BindableProperty SelectedValuePathProperty = BindableProperty.Create(nameof(SelectedValuePath), typeof(string), typeof(PopupSelector));
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(nameof(SelectedValue), typeof(object), typeof(PopupSelector), null, BindingMode.TwoWay);
    public static readonly BindableProperty DisplayFormatProperty = BindableProperty.Create(nameof(DisplayFormat), typeof(string), typeof(PopupSelector));

    public static readonly BindableProperty ShowClearFieldImageProperty = BindableProperty.Create(nameof(ShowClearFieldImage), typeof(bool), typeof(PopupSelector), false);
    public static readonly BindableProperty DropDownWidthProperty = BindableProperty.Create(nameof(DropDownWidth), typeof(double), typeof(PopupSelector), -1.0);
    public static readonly BindableProperty DropDownHeightProperty = BindableProperty.Create(nameof(DropDownHeight), typeof(double), typeof(PopupSelector), 200.0);
    public static readonly BindableProperty DropdownCornerRadiusProperty = BindableProperty.Create(nameof(DropdownCornerRadius), typeof(CornerRadius), typeof(PopupSelector), new CornerRadius(10), propertyChanged: CornerRadiusChanged);
    public static readonly BindableProperty DropdownTextColorProperty = BindableProperty.Create(nameof(DropdownTextColor), typeof(Color), typeof(PopupSelector), Colors.Black);
    public static readonly BindableProperty DropdownBackgroundColorProperty = BindableProperty.Create(nameof(DropdownBackgroundColor), typeof(Color), typeof(PopupSelector), Colors.White);
    public static readonly BindableProperty DropdownBorderColorProperty = BindableProperty.Create(nameof(DropdownBorderColor), typeof(Color), typeof(PopupSelector), Colors.DarkGrey);
    public static readonly BindableProperty DropdownBorderWidthProperty = BindableProperty.Create(nameof(DropdownBorderWidth), typeof(double), typeof(PopupSelector), 1.0);
    public static readonly BindableProperty DropdownClearFieldImageSourceProperty = BindableProperty.Create(nameof(DropdownClearFieldImageSource), typeof(string), typeof(PopupSelector), "x_circle.png");
    public static readonly BindableProperty DropdownClosedImageSourceProperty = BindableProperty.Create(nameof(DropdownClosedImageSource), typeof(string), typeof(PopupSelector), "chevron_right.png");
    public static readonly BindableProperty DropdownOpenImageSourceProperty = BindableProperty.Create(nameof(DropdownOpenImageSource), typeof(string), typeof(PopupSelector), "chevron_down.png");
    public static readonly BindableProperty DropdownImageTintProperty = BindableProperty.Create(nameof(DropdownImageTint), typeof(Color), typeof(PopupSelector));
    public static readonly BindableProperty DropdownShadowProperty = BindableProperty.Create(nameof(DropdownShadow), typeof(bool), typeof(PopupSelector), true);
    public static readonly BindableProperty DropdownSeparatorProperty = BindableProperty.Create(nameof(DropdownSeparator), typeof(bool), typeof(PopupSelector), true);

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is PopupSelector selector) {
            // Force update when ItemsSource changes
            if (!selector._isInitializing && selector.SelectedValue != null) {
                selector.UpdateSelectedItemFromValue();
            }
        }
    }
    
    public PopupSelectorTypeEnum SelectorType {
        get => (PopupSelectorTypeEnum)GetValue(SelectorTypeProperty);
        set => SetValue(SelectorTypeProperty, value);
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

    public bool ShowAnchor {
        get => (bool)GetValue(ShowAnchorProperty);
        set => SetValue(ShowAnchorProperty, value);
    }

    public Thickness InnerMargin {
        get => (Thickness)GetValue(InnerMarginProperty);
        set => SetValue(InnerMarginProperty, value);
    }

    /// <summary>
    ///     Part of the DropDown Configuration: How rounded should the dropdown be?
    ///     The default is 10. Set to 0 for square corners.
    /// </summary>
    public CornerRadius DropdownCornerRadius {
        get => (CornerRadius)GetValue(DropdownCornerRadiusProperty);
        set => SetValue(DropdownCornerRadiusProperty, value);
    }

    /// <summary>
    ///     Specifies the width of the dropdown menu. If set to a value greater than 0,
    ///     the dropdown will use the specified width. Otherwise, it defaults to the
    ///     width of the main button layout.
    /// </summary>
    public double DropDownWidth {
        get => (double)GetValue(DropDownWidthProperty);
        set => SetValue(DropDownWidthProperty, value);
    }

    /// <summary>
    ///     Defines the height of the dropdown component. This value determines
    ///     the vertical size of the dropdown when it is displayed.
    /// </summary>
    public double DropDownHeight {
        get => (double)GetValue(DropDownHeightProperty);
        set => SetValue(DropDownHeightProperty, value);
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
    ///     Specifies the text color for items displayed in the dropdown list.
    ///     This property determines the `TextColor` of labels rendered within the dropdown.
    /// </summary>
    public Color DropdownTextColor {
        get => (Color)GetValue(DropdownTextColorProperty) ?? Colors.Black;
        set => SetValue(DropdownTextColorProperty, value);
    }

    /// <summary>
    ///     Gets or sets the border color of the dropdown. This property defines the color of the border
    ///     surrounding the dropdown menu when it is displayed.
    /// </summary>
    public Color DropdownBorderColor {
        get => (Color)GetValue(DropdownBorderColorProperty) ?? Colors.DarkGrey;
        set => SetValue(DropdownBorderColorProperty, value);
    }

    /// <summary>
    ///     Represents a brush that is derived from the <c>DropdownBorderColor</c> property,
    ///     used to define the border color as a solid brush for the dropdown box.
    /// </summary>
    public SolidColorBrush DropdownBorderColorBrush => new(DropdownBorderColor);

    /// <summary>
    ///     Gets or sets the width of the border surrounding the dropdown.
    /// </summary>
    public double DropdownBorderWidth {
        get => (double)GetValue(DropdownBorderWidthProperty);
        set => SetValue(DropdownBorderWidthProperty, value);
    }

    /// <summary>
    ///     Gets or sets the background color of the dropdown list.
    ///     This property determines the visual background appearance
    ///     of the dropdown items when displayed.
    /// </summary>
    public Color DropdownBackgroundColor {
        get => (Color)GetValue(DropdownBackgroundColorProperty) ?? Colors.Gainsboro;
        set => SetValue(DropdownBackgroundColorProperty, value);
    }

    /// <summary>
    ///     Gets or sets the image source used to represent the closed state of the dropdown.
    /// </summary>
    public string DropdownClearFieldImageSource {
        get => (string)GetValue(DropdownClearFieldImageSourceProperty);
        set => SetValue(DropdownClearFieldImageSourceProperty, value);
    }

    public bool ShowClearFieldImage {
        get => (bool)GetValue(ShowClearFieldImageProperty);
        set => SetValue(ShowClearFieldImageProperty, value);
    }

    /// <summary>
    ///     Gets or sets the image source used to represent the closed state of the dropdown.
    /// </summary>
    public string DropdownClosedImageSource {
        get => (string)GetValue(DropdownClosedImageSourceProperty);
        set => SetValue(DropdownClosedImageSourceProperty, value);
    }

    /// <summary>
    ///     Represents the image source to be used when the dropdown is in its open state.
    ///     This property allows customization of the visual appearance by specifying
    ///     the file path or resource name of the image displayed.
    /// </summary>
    public string DropdownOpenImageSource {
        get => (string)GetValue(DropdownOpenImageSourceProperty);
        set => SetValue(DropdownOpenImageSourceProperty, value);
    }

    /// <summary>
    ///     Specifies the tint color applied to the dropdown indicator image.
    ///     This property allows customization of the arrow image's color, enhancing visual consistency
    ///     with the application's theme or design requirements.
    /// </summary>
    public Color? DropdownImageTint {
        get => (Color?)GetValue(DropdownImageTintProperty);
        set => SetValue(DropdownImageTintProperty, value);
    }

    /// <summary>
    ///     Determines whether the dropdown box displays a shadow effect.
    /// </summary>
    public bool DropdownShadow {
        get => (bool)GetValue(DropdownShadowProperty);
        set => SetValue(DropdownShadowProperty, value);
    }

    /// <summary>
    ///     Indicates whether a separator is displayed within the dropdown.
    /// </summary>
    public bool DropdownSeparator {
        get => (bool)GetValue(DropdownSeparatorProperty);
        set => SetValue(DropdownSeparatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path that is used to get the display value for each item in the ItemsSource.
    /// When null or empty, the item's ToString() method is used.
    /// </summary>
    public string DisplayMemberPath {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path that is used to get the value from the selected item.
    /// When null or empty, the entire object is used as the value.
    /// </summary>
    public string SelectedValuePath {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that is extracted from the selected item using the SelectedValuePath.
    /// This is what gets bound to your model property (e.g., the ID).
    /// </summary>
    public object? SelectedValue {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public string DisplayFormat {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    public SeparatorVisibility DropdownSeparatorVisibility => DropdownSeparator ? SeparatorVisibility.Default : SeparatorVisibility.None;
    #endregion
}

public static class ViewExtensions {
    public static Rect GetAbsoluteBounds(this IView view) {
        var element = view as VisualElement;
        var x = element?.X ?? 0;
        var y = element?.Y ?? 0;
        var parent = element?.Parent as VisualElement;
        while (parent != null) {
            x += parent.X;
            y += parent.Y;
            parent = parent.Parent as VisualElement;
        }
        return new Rect(x, y, element?.Width ?? 0, element?.Height ?? 0);
    }
}

public class SelectedItemToDisplayTextConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values?.Length != 4) return string.Empty;

        var selectedItem = values[0];
        var placeholder = values[1] as string ?? string.Empty;
        var displayMemberPath = values[2] as string;
        var displayFormat = values[3] as string;

        // If no item is selected, show placeholder
        if (selectedItem == null) {
            return placeholder;
        }

        // If we have a display format, use it
        if (!string.IsNullOrEmpty(displayFormat)) {
            return PopupSelector.FormatObject(selectedItem, displayFormat) ?? placeholder;
        }

        // If we have a display member path, use it
        if (!string.IsNullOrEmpty(displayMemberPath)) {
            var displayValue = PopupSelector.GetPropertyValue(selectedItem, displayMemberPath);
            return displayValue?.ToString() ?? placeholder;
        }

        // Default: use ToString()
        return selectedItem.ToString() ?? placeholder;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}