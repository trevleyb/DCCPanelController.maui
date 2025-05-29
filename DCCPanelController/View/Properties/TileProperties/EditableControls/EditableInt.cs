using System.Globalization;
using System.Reflection;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableInt(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public int MinValue { get; set; } = 0;   // used for Int (Minimum Value)
    public int MaxValue { get; set; } = 999; // used for Int (Maximum Value)

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
                Margin = new Thickness(10, 5, 10, 5),
                Text = info.GetValue(owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });

            var stepperUpDown = new Stepper {
                Minimum = 0,  // Define the stepper min value if needed
                Maximum = 90, // Define the stepper max value if needed
                HeightRequest = 30,
                Increment = 1, // Increment/decrement step
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };

            if (int.TryParse(dataCell.Text, out var initialStepperValue)) {
                stepperUpDown.Value = initialStepperValue;
            }

            stepperUpDown.ValueChanged += (s, e) => { dataCell.Text = e?.NewValue.ToString(CultureInfo.InvariantCulture) ?? "0"; };

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
            Console.WriteLine($"Unable to create a Int: {e.Message}");
            return null;
        }
    }
}