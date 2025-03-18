using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableOpacity : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new HorizontalStackLayout();
            cell.VerticalOptions = LayoutOptions.Center;
            cell.HorizontalOptions = LayoutOptions.Start;
            var dataCell = new Entry {
                BindingContext = owner,
                WidthRequest = 75,
                HeightRequest = 30,
                Placeholder = attribute.Label,
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(10, 0, 10, 0),
                Text = info.GetValue(owner)?.ToString() ?? "0"
            };

            dataCell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            var stepperUpDown = new Stepper {
                Minimum = 0,  // Define the stepper min value if needed
                Maximum = 1, // Define the stepper max value if needed
                HeightRequest = 30,
                Increment = 0.05, // Increment/decrement step
                HorizontalOptions = LayoutOptions.End
            };

            if (int.TryParse(dataCell.Text, out var initialStepperValue)) {
                stepperUpDown.Value = initialStepperValue;
            }

            stepperUpDown.ValueChanged += (s, e) => { dataCell.Text = e?.NewValue.ToString("_00%", CultureInfo.InvariantCulture) ?? "100%"; };
            dataCell.TextChanged += (s, e) => {
                if (int.TryParse(e.NewTextValue, out var parsedValue)) {
                    stepperUpDown.Value = parsedValue;
                }
            };

            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell, owner, info, attribute);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Int: {e.Message}");
            return null;
        }
    }
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
}