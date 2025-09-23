using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Components;

[ContentProperty(nameof(Items))]
public class InlineToolbar : ContentView {

    readonly HorizontalStackLayout _stack;
    
    public InlineToolbar() {
        _stack = new HorizontalStackLayout {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Fill
        };
        Content = _stack;
        Items.CollectionChanged += OnItemsChanged;
    }

    // Define items directly inside <ui:InlineToolbar>…</ui:InlineToolbar>
    public ObservableCollection<InlineToolbarItem> Items { get; } = new();

    public static readonly BindableProperty SpacingProperty       = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(InlineToolbar), 8.0, propertyChanged: (b, o, n) => ((InlineToolbar)b)._stack.Spacing = (double)n);
    public static readonly BindableProperty IconSizeProperty      = BindableProperty.Create(nameof(IconSize), typeof(double), typeof(InlineToolbar), 24.0);
    public static readonly BindableProperty ShowTextProperty      = BindableProperty.Create(nameof(ShowText), typeof(bool), typeof(InlineToolbar), true);
    public static readonly BindableProperty ButtonPaddingProperty = BindableProperty.Create(nameof(ButtonPadding), typeof(Thickness), typeof(InlineToolbar), new Thickness(8, 6));
    public static readonly BindableProperty ButtonStyleProperty   = BindableProperty.Create(nameof(ButtonStyle), typeof(Style), typeof(InlineToolbar), default(Style));

    public double Spacing {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
    
    public double IconSize {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
    
    public bool ShowText {
        get => (bool)GetValue(ShowTextProperty);
        set => SetValue(ShowTextProperty, value);
    }
    
    public Thickness ButtonPadding {
        get => (Thickness)GetValue(ButtonPaddingProperty);
        set => SetValue(ButtonPaddingProperty, value);
    }
    
    public Style? ButtonStyle {
        get => (Style?)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        foreach (var it in Items) it.BindingContext = BindingContext;
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.OldItems != null) {
            foreach (InlineToolbarItem it in e.OldItems) {
                it.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        if (e.NewItems != null) {
            foreach (InlineToolbarItem it in e.NewItems) { 
                it.BindingContext = BindingContext;
                it.PropertyChanged += OnItemPropertyChanged;
            }
        }
        Rebuild();
    }

    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(InlineToolbarItem.IconImageSource)
            or nameof(InlineToolbarItem.IsEnabled)
            or nameof(InlineToolbarItem.Command)
            or nameof(InlineToolbarItem.CommandParameter)
            or nameof(InlineToolbarItem.TextColor)
            or nameof(InlineToolbarItem.TextSize)
            or nameof(InlineToolbarItem.Text)) {
            Rebuild();
        }
    }

    void Rebuild() {
        _stack.Children.Clear();
        foreach (var it in Items) _stack.Children.Add(BuildButton(it));
    }

   Microsoft.Maui.Controls.View BuildButton(InlineToolbarItem it) {
        var btn = new ImageButton {
            BindingContext = it,
            BackgroundColor = Colors.Transparent,
            Style = ButtonStyle,
            WidthRequest = IconSize,
            HeightRequest = IconSize,
            Padding = ButtonPadding,
        };
        btn.SetBinding(ImageButton.IsEnabledProperty, nameof(InlineToolbarItem.IsEnabled));
        btn.SetBinding(ImageButton.SourceProperty, nameof(InlineToolbarItem.IconImageSource));
        btn.SetBinding(ImageButton.CommandProperty, nameof(InlineToolbarItem.Command));
        btn.SetBinding(ImageButton.CommandParameterProperty, nameof(InlineToolbarItem.CommandParameter));
        btn.SetBinding(SemanticProperties.DescriptionProperty, nameof(InlineToolbarItem.Text));
        #if WINDOWS
        btn.SetBinding(TooltipProperties.TextProperty, nameof(InlineToolbarItem.Text));
        #endif

        return btn;
    }
}