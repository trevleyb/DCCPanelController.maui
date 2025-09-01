using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Views.Properties.TileProperties.EditableControls;

public class EditableInt(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public int MinValue { get; set; } = 0;   // used for Int (Minimum Value)
    public int MaxValue { get; set; } = 999; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new HorizontalStackLayout {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            var dataCell = new Label {
                BindingContext = owner,
                WidthRequest = 75,
                HeightRequest = 30,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 10, 5),
            };

            var stepperUpDown = new Stepper {
                Minimum = 0,  // Define the stepper min value if needed
                Maximum = 90, // Define the stepper max value if needed
                HeightRequest = 30,
                Increment = 1, // Increment/decrement step
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };

            stepperUpDown.SetBinding(Stepper.ValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            stepperUpDown.ValueChanged += (s, e) => {
                dataCell.Text = e?.NewValue.ToString(CultureInfo.InvariantCulture) ?? "0";
                var originalValue = info.GetValue(owner);
                SetModified(originalValue == null || !Equals(e?.NewValue, originalValue));
            };
            dataCell.Text = stepperUpDown.Value.ToString(CultureInfo.InvariantCulture) ?? "0";
            cell.Children.Add(stepperUpDown);
            cell.Children.Add(dataCell);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Int: {Message}",e.Message);
            return null;
        }
    }
}