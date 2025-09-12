using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Helpers;

public class BindingProxy : BindableObject
{
    public static readonly BindableProperty DataProperty =
        BindableProperty.Create(nameof(Data), typeof(object), typeof(BindingProxy), default(object));

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}
