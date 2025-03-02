using System.Diagnostics;
using System.Globalization;
using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableDoubleAttribute : EditableAttribute, IEditableAttribute {
    public double MinValue { get; set; } = 0;  // used for Int (Minimum Value)
    public double MaxValue { get; set; } = 10; // used for Int (Maximum Value)

    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new HorizontalStackLayout();

            var dataCell = new Entry {
                BindingContext = value.Owner,
                WidthRequest = 75,
                Placeholder = value.EditableAttribute.Description,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(0, 0, 10, 0),
                Text = value.Info.GetValue(value.Owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });

            var attr = value.EditableAttribute as EditableIntAttribute;

            var stepperUpDown = new Stepper {
                Minimum = attr?.MinValue ?? 0,  // Define the stepper min value if needed
                Maximum = attr?.MaxValue ?? 10, // Define the stepper max value if needed
                HeightRequest = 20,
                Increment = 0.25, // Increment/decrement step
                HorizontalOptions = LayoutOptions.End
            };

            if (int.TryParse(dataCell.Text, out var initialStepperValue)) {
                stepperUpDown.Value = initialStepperValue;
            }

            stepperUpDown.ValueChanged += (s, e) => { dataCell.Text = e?.NewValue.ToString("00.00", CultureInfo.InvariantCulture) ?? "1.00"; };

            dataCell.TextChanged += (s, e) => {
                if (int.TryParse(e.NewTextValue, out var parsedValue)) {
                    stepperUpDown.Value = parsedValue;
                }
            };

            cell.Children.Add(dataCell);
            cell.Children.Add(stepperUpDown);
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Int: {e.Message}");
            return null;
        }
    }
}