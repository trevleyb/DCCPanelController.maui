using System.Globalization;
using System.Reflection;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableOpacity(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        var originalValue = (int)((double)(info?.GetValue(owner) ?? 0) * 100);
        try {
            var cell = new HorizontalStackLayout();
            cell.VerticalOptions = LayoutOptions.Center;
            cell.HorizontalOptions = LayoutOptions.Start;
            var dataCell = new Entry {
                BindingContext = owner,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 75,
                HeightRequest = 30,
                Margin = new Thickness(10, 0, 10, 0),
                Text = ConvertOpacityToPercentage((double)(info?.GetValue(owner) ?? 0.00))
            };
            
            var stepperUpDown = new Stepper {
                Minimum = 0, // Define the stepper min value if needed
                Maximum = 1, // Define the stepper max value if needed
                HeightRequest = 30,
                Increment = 0.05, // Increment/decrement step
                HorizontalOptions = LayoutOptions.End,
            };
            stepperUpDown.SetBinding(Stepper.ValueProperty, new Binding(info?.Name) { Source = owner, Mode = BindingMode.TwoWay });
            
            stepperUpDown.ValueChanged += (s, e) => {
                dataCell.Text = dataCell.Text = ConvertOpacityToPercentage(stepperUpDown.Value);
                var testValue = (int)(stepperUpDown.Value * 100); 
                SetModified(testValue != originalValue);
            };
            
            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Int: {e.Message}");
            return null;
        }
    }

    private static string ConvertOpacityToPercentage(double opacity) {
        return opacity.ToString("0%", CultureInfo.InvariantCulture);
    }

    private static double ConvertToPercentageToOpacity(string percentage) {
        if (percentage.EndsWith("%") && double.TryParse(percentage.TrimEnd('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue)) {
            return Math.Clamp(parsedValue / 100.0, 0.0, 1.0);
        }
        Console.WriteLine($"Invalid percentage format: {percentage}. Ensure the value ends with '%' and represents a valid number.");
        return 0.0;
    }
}