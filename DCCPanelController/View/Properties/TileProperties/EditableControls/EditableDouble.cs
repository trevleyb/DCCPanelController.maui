using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableDouble(string label, string description = "", int order = 0, string? group = null, double minValue = 0, double maxValue = 10, double stepValue = 0.1)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public double MinValue { get; set; } = minValue;   // used for Int (Minimum Value) 
    public double MaxValue { get; set; } = maxValue;   // used for Int (Maximum Value)
    public double StepValue { get; set; } = stepValue; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
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
                Text = info.GetValue(owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            var stepperUpDown = new Stepper {
                Minimum = MinValue,    // Define the stepper min value if needed
                Maximum = MaxValue,    // Define the stepper max value if needed
                Increment = StepValue, // Increment/decrement step
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            if (int.TryParse(dataCell.Text, out var initialStepperValue)) {
                stepperUpDown.Value = initialStepperValue;
            }

            stepperUpDown.ValueChanged += (s, e) => { dataCell.Text = e?.NewValue.ToString("0.00", CultureInfo.InvariantCulture) ?? "1.00"; };
            dataCell.TextChanged += (s, e) => {
                propertyModified?.Invoke(info.Name);
                if (int.TryParse(e.NewTextValue, out var parsedValue)) {
                    stepperUpDown.Value = parsedValue;
                }
            };

            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Int: {e.Message}");
            return null;
        }
    }
}