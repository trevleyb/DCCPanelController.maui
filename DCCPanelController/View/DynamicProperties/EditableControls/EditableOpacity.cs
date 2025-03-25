using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableOpacity(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new HorizontalStackLayout();
            cell.VerticalOptions = LayoutOptions.Center;
            cell.HorizontalOptions = LayoutOptions.Start;
            var dataCell = new Entry {
                BindingContext = owner,
                WidthRequest = 75,
                HeightRequest = 30,
                Placeholder = Label,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(10, 0, 10, 0),
                Text = ConvertOpacityToPercentage((double)(info.GetValue(owner) ?? 0.00))
            };
            var stepperUpDown = new Stepper {
                Minimum = 0,  // Define the stepper min value if needed
                Maximum = 1, // Define the stepper max value if needed
                HeightRequest = 30,
                Increment = 0.05, // Increment/decrement step
                HorizontalOptions = LayoutOptions.End,
                Value = ConvertToPercentageToOpacity(dataCell.Text)
            };

            stepperUpDown.ValueChanged += (s, e) => {
                dataCell.Text = dataCell.Text = ConvertOpacityToPercentage(stepperUpDown.Value);
                info.SetValue(owner, stepperUpDown.Value);
                Console.WriteLine($"Stepper Value Changed: {stepperUpDown.Value} = {dataCell.Text}");
            };
            
            dataCell.TextChanged += (s, e) => {
                stepperUpDown.Value = ConvertToPercentageToOpacity(dataCell.Text);
                info.SetValue(owner, stepperUpDown.Value);
                Console.WriteLine($"Stepper Text Changed: {stepperUpDown.Value} = {dataCell.Text}");
            };
            
            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Int: {e.Message}");
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