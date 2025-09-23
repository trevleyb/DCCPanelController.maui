using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Components;

public class InlineToolbarItem : BindableObject {

    public static readonly BindableProperty NameProperty             = BindableProperty.Create(nameof(Name), typeof(string), typeof(InlineToolbarItem));
    public static readonly BindableProperty TextProperty             = BindableProperty.Create(nameof(Text), typeof(string), typeof(InlineToolbarItem));
    public static readonly BindableProperty TextSizeProperty         = BindableProperty.Create(nameof(TextSize), typeof(int), typeof(InlineToolbarItem),8);
    public static readonly BindableProperty TextColorProperty        = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(InlineToolbarItem), Colors.Black);
    public static readonly BindableProperty IconImageSourceProperty  = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(InlineToolbarItem));
    public static readonly BindableProperty IsEnabledProperty        = BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(InlineToolbarItem), true);
    public static readonly BindableProperty CommandProperty          = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(InlineToolbarItem));
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(InlineToolbarItem));

    public string? Name {
        get => (string?)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
    
    public string? Text {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int TextSize {
        get => (int)GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }
    
    public Color TextColor {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }
    
    public ImageSource? IconImageSource {
        get => (ImageSource?)GetValue(IconImageSourceProperty);
        set => SetValue(IconImageSourceProperty, value);
    }
    
    public bool IsEnabled {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }
    
    public ICommand? Command {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public object? CommandParameter {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}