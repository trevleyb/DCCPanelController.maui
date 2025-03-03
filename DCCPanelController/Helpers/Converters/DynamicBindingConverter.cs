using System.Globalization;
using DCCPanelController.View.PropertyPages;

namespace DCCPanelController.Helpers.Converters;

public class DynamicBindingConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is PanelPropertyViewModel viewModel && parameter is Binding bindingParam && !string.IsNullOrEmpty(bindingParam.Path)) {
            var bindingPath = bindingParam.Path;
            var property = viewModel.GetType().GetProperty(bindingPath);
            return property?.GetValue(viewModel) ?? "";
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is Color color && parameter is string bindingPath && targetType.IsAssignableFrom(typeof(PanelPropertyViewModel))) {
            var property = targetType.GetProperty(bindingPath);
            property?.SetValue(null, color); // Assuming static ViewModel or manageable context
        }

        return "";
    }
}