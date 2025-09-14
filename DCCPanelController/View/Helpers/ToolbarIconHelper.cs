namespace DCCPanelController.View.Helpers;

public static class ToolbarIconHelper {
    public static void BindIcon(ToolbarItem item,
        string isActivePath, // e.g. "IsCurrentToolActive"
        string baseName      // e.g. "move" | "size" | "clone"
    ) {
        var converter = (IValueConverter)App.Current.Resources["BoolToIcon"];
        item.RemoveBinding(MenuItem.IconImageSourceProperty);
        item.SetBinding(MenuItem.IconImageSourceProperty, new Binding(isActivePath) {
            Mode = BindingMode.OneWay,
            Converter = converter,
            ConverterParameter = baseName,
        });
    }
}