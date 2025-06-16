using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableID(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    private IEntityID? _entity;

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            _entity = owner as IEntityID;
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = "Unique ID",
                Keyboard = Keyboard.Text,
                WidthRequest = 100,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };
            cell.TextChanged += CellOnTextChanged;
            cell.Completed += CellOnCompleted;
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Entry.Text)) {
                    if (sender is Entry item) {
                        SetModified(item.Text != (string)Value!);
                    }
                }
            };
            return CreateGroupCell(cell);
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
        if (sender is Entry entry) entry.TextColor = isValid ? Colors.Black : Colors.Red;
        SetModified(true);
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