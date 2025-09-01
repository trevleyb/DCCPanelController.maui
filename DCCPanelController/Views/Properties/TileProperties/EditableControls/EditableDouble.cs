using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Views.Properties.TileProperties.EditableControls;

public class EditableDouble(string label, string description = "", int order = 0, string? group = null, double minValue = 0, double maxValue = 10, double stepValue = 0.1)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public double MinValue { get; set; } = minValue;   // used for Int (Minimum Value) 
    public double MaxValue { get; set; } = maxValue;   // used for Int (Maximum Value)
    public double StepValue { get; set; } = stepValue; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new HorizontalStackLayout();
            cell.VerticalOptions = LayoutOptions.Center;
            cell.HorizontalOptions = LayoutOptions.Start;
            var dataCell = new Label {
                BindingContext = owner,
                WidthRequest = 75,
                HeightRequest = 30,
                Margin = new Thickness(10, 10, 10, 0),
                Text = info.GetValue(owner)?.ToString() ?? "0"
            };

            var stepperUpDown = new Stepper {
                Minimum = MinValue,    // Define the stepper min value if needed
                Maximum = MaxValue,    // Define the stepper max value if needed
                Increment = StepValue, // Increment/decrement step
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            stepperUpDown.SetBinding(Stepper.ValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            stepperUpDown.ValueChanged += (s, e) => {
                dataCell.Text = e?.NewValue.ToString("0.00", CultureInfo.InvariantCulture) ?? "1.00";
                var originalValue = info.GetValue(owner);
                SetModified(originalValue == null || !Equals(e?.NewValue, originalValue));
            };

            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Double: {Message}",e.Message);
            return null;
        }
    }
}