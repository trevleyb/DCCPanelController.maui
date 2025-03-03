using System.Diagnostics;
using System.Globalization;
using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableIntAttribute : EditableAttribute, IEditableAttribute {
    public int MinValue { get; set; } = 0;   // used for Int (Minimum Value)
    public int MaxValue { get; set; } = 999; // used for Int (Maximum Value)

    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new HorizontalStackLayout();

            var dataCell = new Entry {
                BindingContext = value.Owner,
                WidthRequest = 75,
                HeightRequest = 25,
                Placeholder = value.EditableAttribute.Description,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(5, 5, 10, 5),
                Text = value.Info.GetValue(value.Owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });

            var attr = value.EditableAttribute as EditableIntAttribute;

            var stepperUpDown = new Stepper {
                Minimum = attr?.MinValue ?? 0,  // Define the stepper min value if needed
                Maximum = attr?.MaxValue ?? 99, // Define the stepper max value if needed
                HeightRequest = 20,
                Increment = 1, // Increment/decrement step
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };

            if (int.TryParse(dataCell.Text, out var initialStepperValue)) {
                stepperUpDown.Value = initialStepperValue;
            }

            stepperUpDown.ValueChanged += (s, e) => { dataCell.Text = e?.NewValue.ToString(CultureInfo.InvariantCulture) ?? "0"; };

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