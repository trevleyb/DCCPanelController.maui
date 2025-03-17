using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableOpacity : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new HorizontalStackLayout();
            cell.VerticalOptions = LayoutOptions.Center;
            var dataCell = new Entry {
                BindingContext = owner,
                WidthRequest = 75,
                HeightRequest = 25,
                Placeholder = attribute.Label,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(0, 0, 10, 0),
                Text = info.GetValue(owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            var stepperUpDown = new Stepper {
                Minimum = 0,  // Define the stepper min value if needed
                Maximum = 1, // Define the stepper max value if needed
                HeightRequest = 20,
                Increment = 0.05, // Increment/decrement step
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