using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableID(string label, string description = "", int order = 0, string? group = null, int width = 300)
    : EditableProperty(label, description, order, group), IEditableProperty {
    private IEntityID? _entity;

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            _entity = owner as IEntityID;
            var box = new Border() {
                Margin = new Thickness(5, 5, 5, 5),
                WidthRequest = width,
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush((Color?)Application.Current?.Resources["Primary"] ?? Colors.Black),
                StrokeShape = new RoundRectangle {
                    CornerRadius = new CornerRadius(10) // All corners rounded with radius 10
                }
            };
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = "Unique ID",
                Keyboard = Keyboard.Text,
                WidthRequest = 300,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };
            cell.TextChanged += CellOnTextChanged;
            cell.Completed += CellOnCompleted;
            Console.WriteLine($"Creating Binding????");
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            Console.WriteLine($"Done");
            box.Content = cell;
            return CreateGroupCell(box);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }

    private void CellOnCompleted(object? sender, EventArgs e) {
        if (sender is Entry entry && _entity is { } entity) {
            if (!IsIDValid(entry.Text)) _entity.Id = entity.NextID;
        }
    }

    private void CellOnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = IsIDValid(e.NewTextValue);
        if (sender is Entry entry) {
            entry.TextColor = isValid ? Colors.Black : Colors.Red;
            if (Value is string { } value) {
                var hasChanged = !entry.Text.Equals(value);
                SetModified(hasChanged);
            }
        }
    }

    private bool IsIDValid(string value) {
        var isValid = true;
        if (_entity is { AllIDs: { } ids }) {
            var conflictingEntities = ids?.Where(entity => entity.Id == value).ToArray() ?? [];
            isValid = conflictingEntities.Length is 0 or 1;
        }
        return isValid;
    }
}